using Avalonia.Controls;
using Avalonia.Markup.Declarative;
using Nodify;
using Avalonia.Media;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using System;
using System.Windows.Input;
using System.Linq;
using DynamicData;
using System.Collections.Generic;
using System.Reactive.Linq;
using TEA;
using Avalonia.Styling;
using Avalonia.Data;
using System.ComponentModel;
using TEA.MVVM;

namespace TransportSimulator.Views;

public enum LogicKind {
    And,
    Or,
    Not,
    Nand,
    Nor,
    Xor
}

public record struct NodeModel(
    uint NodeID,
    Point Location,
    LogicKind Logic,
    string Title,
    List<ConnectorModel> Input,
    List<ConnectorModel> Output
);

/// <summary>
/// 
/// </summary>
/// <param name="NodeID">どのノードのコネクタかを表します。</param>
/// <param name="IOIndex">リストのインデックス</param>
/// <param name="Anchor"></param>
/// <param name="IsConnected"></param>
/// <param name="Title"></param>
public record struct ConnectorModel(
    ConnectorID ID,
    Point Anchor,
    bool IsConnected,
    string Title
);

public class DelegateCommand<T> : ICommand {
    readonly Action<T> action;
    readonly Func<T, bool>? condition;

    public event EventHandler? CanExecuteChanged;

    public DelegateCommand(Action<T> action, Func<T, bool>? executeCondition = default) {
        this.action = action ?? throw new ArgumentNullException(nameof(action));
        condition = executeCondition;
    }

    public bool CanExecute(object? parameter) {
        if (parameter is T value) {
            return condition?.Invoke(value) ?? true;
        }

        return condition?.Invoke(default!) ?? true;
    }

    public void Execute(object? parameter) {
        if (parameter is T value) {
            action(value);
        } else {
            action(default!);
        }
    }

    public void RaiseCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, new EventArgs());
}

public enum ConnectionDir {
    Input,
    Output
}

public record struct ConnectorID(uint NodeID, ConnectionDir Dir, int IOIndex);

/// <summary>
/// 2つのノードをつなぐコネクターのIDを表します。
/// </summary>
public record struct ConnectionID(
    ConnectorID Source,
    ConnectorID Target);

public interface ICircuitDesignerMessage { }

/// <summary>
/// ノードを追加します。
/// </summary>
public record OnAddNodeMessage : ICircuitDesignerMessage;

/// <summary>
/// 指定されたノードに接続されたコネクターの位置が変化したことを通知します。
/// </summary>
public record OnChangedNodeAnchorMessage(ConnectorID ID, Point NewPosition) : ICircuitDesignerMessage;

/// <summary>
/// 指定されたノードが新しい位置に移動したことを通知します。
/// </summary>
public record OnChangedNodeLocationMessage(uint NodeID, Point NewPosition) : ICircuitDesignerMessage;

/// <summary>
/// 2つのノードを接続したことを通知します。
/// </summary>
public record OnConnectNodeMessage(ConnectionID ID) : ICircuitDesignerMessage;

/// <summary>
/// ノードを切断したことを通知します。
/// </summary>
public record OnDisconnectNodeMessage(ConnectorID ID) : ICircuitDesignerMessage;

public class CircuitDesignerModel : IUpdate<CircuitDesignerModel, ICircuitDesignerMessage> {
    uint nextNodeID = 0;
    public Dictionary<uint, NodeModel> Nodes { get; private set; } = new();
    public HashSet<ConnectionID> Connections { get; private set; } = new();

    public static CircuitDesignerModel Default => new();

    public CircuitDesignerModel Update(ICircuitDesignerMessage message) {
        return message switch {
            OnAddNodeMessage => AddNode(),
            OnChangedNodeAnchorMessage msg => OnChangedNodeAnchor(msg),
            OnChangedNodeLocationMessage msg => ChangeNodeLocation(msg),
            OnConnectNodeMessage msg => Connect(msg),
            OnDisconnectNodeMessage msg => Disconnect(msg),
            _ => this
        };
    }

    CircuitDesignerModel AddNode() {
        uint nodeID = GenerateNextNodeID(ref nextNodeID);
        var newNode = new NodeModel(
            NodeID: nodeID,
            Location: new Point(0, 0),
            Logic: LogicKind.And,
            Title: "Welcome",
            Input: new List<ConnectorModel> {
                new ConnectorModel(new ConnectorID(nodeID, ConnectionDir.Input, 0), new Point(0, 0), false, "In")
            },
            Output: new List<ConnectorModel> {
                new ConnectorModel(new ConnectorID(nodeID, ConnectionDir.Output, 0), new Point(0, 0), false, "Output")
        });
        Nodes[nodeID] = newNode;
        return this;
    }

    CircuitDesignerModel OnChangedNodeAnchor(OnChangedNodeAnchorMessage msg) {
        if (!Nodes.TryGetValue(msg.ID.NodeID, out var node)) {
            return this;
        }
        List<ConnectorModel> ioList;
        switch (msg.ID.Dir) {
            case ConnectionDir.Input:
                ioList = node.Input;
                break;
            case ConnectionDir.Output:
                ioList = node.Output;
                break;
            default:
                return this;
        }
        if (msg.ID.IOIndex < 0 || ioList.Count <= msg.ID.IOIndex) {
            return this;
        }
        var connector = ioList[msg.ID.IOIndex];
        connector.Anchor = msg.NewPosition;
        ioList[msg.ID.IOIndex] = connector;
        return this;
    }

    CircuitDesignerModel ChangeNodeLocation(OnChangedNodeLocationMessage msg) {
        if (!Nodes.TryGetValue(msg.NodeID, out var node)) {
            return this;
        }
        node.Location = msg.NewPosition;
        Nodes[msg.NodeID] = node;
        return this;
    }

    CircuitDesignerModel Connect(OnConnectNodeMessage msg) {
        // 接続するノードが存在することを確認する
        if (msg.ID.Source.Dir != ConnectionDir.Output ||
            msg.ID.Target.Dir != ConnectionDir.Input ||
            !Nodes.TryGetValue(msg.ID.Source.NodeID, out var sourceNode) ||
            !Nodes.TryGetValue(msg.ID.Target.NodeID, out var targetNode) ||
            sourceNode.Output.Count <= msg.ID.Source.IOIndex ||
            msg.ID.Source.IOIndex < 0 ||
            targetNode.Input.Count <= msg.ID.Target.IOIndex ||
            msg.ID.Target.IOIndex < 0) {
            return this;
        }
        Connections.Add(msg.ID);
        UpdateIsConnected(Nodes, Connections);
        return this;
    }

    /// <summary>
    /// コネクションが接続されているかどうかを更新します。
    /// </summary>
    static void UpdateIsConnected(Dictionary<uint, NodeModel> nodes, HashSet<ConnectionID> connections) {
        // 全ての接続を消す
        foreach (var node in nodes.Values) {
            for (int i = 0; i < node.Input.Count; i++) {
                var connector = node.Input[i];
                connector.IsConnected = false;
                node.Input[i] = connector;
            }
            for (int i = 0; i < node.Output.Count; i++) {
                var connector = node.Output[i];
                connector.IsConnected = false;
                node.Output[i] = connector;
            }
        }
        // 接続情報に従い接続しているかを更新する
        foreach (var connection in connections) {
            if (nodes.TryGetValue(connection.Source.NodeID, out var sourceNode) &&
                0 <= connection.Source.IOIndex &&
                connection.Source.IOIndex < sourceNode.Output.Count) {
                var connector = sourceNode.Output[connection.Source.IOIndex];
                connector.IsConnected = true;
                sourceNode.Output[connection.Source.IOIndex] = connector;
            }
            if (nodes.TryGetValue(connection.Target.NodeID, out var targetNode) &&
                0 <= connection.Target.IOIndex &&
                connection.Target.IOIndex < targetNode.Input.Count) {
                var connector = targetNode.Input[connection.Target.IOIndex];
                connector.IsConnected = true;
                targetNode.Input[connection.Target.IOIndex] = connector;
            }
        }
    }

    CircuitDesignerModel Disconnect(OnDisconnectNodeMessage msg) {
        // どの接続に対して切断するかを指示されるので、複数削除出来る可能性もあるが、
        // 操作感として一つづつ削除出来たほうが使い勝手がよいと考えて、1つしか削除しない
        var connectionID = Connections.FirstOrDefault(connection =>
            msg.ID.Dir switch {
                ConnectionDir.Output => connection.Source == msg.ID,
                ConnectionDir.Input => connection.Target == msg.ID,
                _ => false
            },
            new ConnectionID(new ConnectorID(0, ConnectionDir.Input, 0), new ConnectorID(0, ConnectionDir.Input, 0)));
        if (connectionID.Source.NodeID != 0 || connectionID.Target.NodeID != 0) {
            Connections.Remove(connectionID);
            UpdateIsConnected(Nodes, Connections);
        }
        return this;
    }

    uint GenerateNextNodeID(ref uint nextNodeID) {
        // id 0 は何も設定されていない時様に使うために読み飛ばす
        while (nextNodeID == 0 || Nodes.ContainsKey(nextNodeID)) {
            unchecked {
                nextNodeID++;
            }
        }
        uint nodeID = nextNodeID;
        nextNodeID++;
        return nodeID;
    }

    public Point GetConnectorAnchor(ConnectorID id) {
        return id.Dir switch {
            ConnectionDir.Input =>
                Nodes.TryGetValue(id.NodeID, out var node) &&
                id.IOIndex < node.Input.Count
                ? node.Input[id.IOIndex].Anchor
                : new Point(0, 0),
            ConnectionDir.Output => Nodes.TryGetValue(id.NodeID, out var node) &&
                id.IOIndex < node.Output.Count
                ? node.Output[id.IOIndex].Anchor
                : new Point(0, 0),
            _ => new Point(0, 0)
        };
    }

    public Point GetConnectionDestinationAnchor(ConnectionID id) {
        return Nodes.TryGetValue(id.Target.NodeID, out var node) &&
            id.Target.IOIndex < node.Output.Count
            ? node.Output[id.Target.IOIndex].Anchor
            : new Point(0, 0);
    }
}

public interface IBindableValueRenderer<TState> : IRender<TState> { }

public interface IReadOnlyBindableValue<TState, TBinding> : IBindableValueRenderer<TState> {
    TBinding Value { get; }
}

public interface IBindableValue<TState, TBinding> : IReadOnlyBindableValue<TState, TBinding> {
    new TBinding Value { set; }
}

/// <summary>
///  レンダリングをした内容をViewに通知するだけのクラス
///  値が同じであれば、Viewへ通知しません。
/// </summary>
public class WritableValue<TState, TBinding> : INotifyPropertyChanged, IReadOnlyBindableValue<TState, TBinding> {
    readonly WriteNotify<TBinding> writeNotify;

    readonly static PropertyChangedEventArgs valueProeprty = new(nameof(Value));

    readonly Func<TState, TBinding> getBindingValue;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///  Viewとのバインディング用変数
    /// </summary>
    public TBinding Value => writeNotify.Value;

    public WritableValue(TBinding initial, Func<TState, TBinding> getBindingValue, IEqualityComparer<TBinding>? comparer = null) {
        writeNotify = new(initial, comparer);
        writeNotify.PropertyChanged += (_, _) => PropertyChanged?.Invoke(this, valueProeprty);
        this.getBindingValue = getBindingValue;
    }

    public WritableValue(TBinding initial, Func<TState, TBinding> getBindingValue, Func<TBinding, TBinding, bool> isSame) {
        writeNotify = new(initial, isSame);
        writeNotify.PropertyChanged += (_, _) => PropertyChanged?.Invoke(this, valueProeprty);
        this.getBindingValue = getBindingValue;
    }

    public void Render(TState state) {
        writeNotify.Render(getBindingValue(state));
    }
}

public class LambdaDispatcher<T> : IDispatcher<T> {
    readonly Action<T> dispatch;

    public LambdaDispatcher(Action<T> dispatch) {
        this.dispatch = dispatch;
    }

    public void Dispatch(T msg) {
        dispatch(msg);
    }
}

public class BindableValue<TState, TBinding> : INotifyPropertyChanged, IRender<TState>, IBindableValue<TState, TBinding> {
    readonly NotifyValue<TBinding> notifyValue;

    readonly static PropertyChangedEventArgs valueProeprty = new(nameof(Value));

    readonly Func<TState, TBinding> getBindingValue;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    ///  Viewとのバインディング用変数
    /// </summary>
    public TBinding Value {
        get => notifyValue.Value;
        set => notifyValue.Value = value;
    }

    public BindableValue(
        TBinding initial,
        Func<TState, TBinding> getBindingValue,
        Action<TBinding> onChanged,
        IEqualityComparer<TBinding>? comparer = null) {
        this.getBindingValue = getBindingValue;
        notifyValue = new(initial, comparer);
        notifyValue.PropertyChanged += (_, _) => PropertyChanged?.Invoke(this, valueProeprty);
        notifyValue.Setup(new LambdaDispatcher<TBinding>(onChanged));
    }

    public BindableValue(
        TBinding initial, Func<TState, TBinding> getBindingValue,
        Action<TBinding> onChanged,
        Func<TBinding, TBinding, bool> isSame) {
        this.getBindingValue = getBindingValue;
        notifyValue = new(initial, isSame);
        notifyValue.PropertyChanged += (_, _) => PropertyChanged?.Invoke(this, valueProeprty);
        notifyValue.Setup(new LambdaDispatcher<TBinding>(onChanged));
    }


    public void Render(TState state) {
        notifyValue.Render(getBindingValue(state));
    }
}

public class BindingObject<TState> : IRender<TState> {
    public IReadOnlyDictionary<string, IBindableValueRenderer<TState>> Bindables { get; }
    public TState State { get; private set; }

    public event Action? OnFinishRender;

    public BindingObject(TState initialState, IReadOnlyDictionary<string, IBindableValueRenderer<TState>> bindables) {
        this.Bindables = bindables;
        State = initialState;
    }

    public void Render(TState state) {
        State = state;
        foreach (var bindable in Bindables.Values) {
            bindable.Render(state);
        }
        OnFinishRender?.Invoke();
    }
}

public static class BindingExtensions {
    public static Func<ObservableCollection<T>> AsObservableCollection<T>(Func<IEnumerable<T>> getSource, Func<T, T, bool>? equals = null) {
        var prevValues = new ObservableCollection<T>();
        equals ??= ((a, b) => EqualityComparer<T>.Default.Equals(a, b));
        return () => {
            var endIndex = prevValues.ApplyToList(getSource(), equals);
            // 配列の要素の移動を最小化するために後ろから削除する
            for (int i = prevValues.Count - 1; endIndex <= i; i--) {
                prevValues.RemoveAt(i);
            }
            return prevValues;
        };
    }
    
    public static int ApplyToBindingObject<TState>(
        this IList<BindingObject<TState>> cachedObject,
        Func<TState, int, BindingObject<TState>> createBindingSetting,
        IEnumerable<TState> state) {
        using var e = state.GetEnumerator();
        int i = 0;
        // キャッシュからrenderを呼び出す
        foreach (var r in cachedObject) {
            if (!e.MoveNext()) {
                return i;
            }
            // ステートがあるうちはrenderに渡す
            r.Render(e.Current);
            i++;
        }
        // 足りない分をインスタンス化する
        for (; e.MoveNext(); i++) {
            var index = i;
            var current = e.Current;
            var render = createBindingSetting(current, i);
            cachedObject.Add(render);
            render.Render(current);
        }
        return i;
    }

    public static Func<ObservableCollection<BindingObject<T>>> AsBindableObservableCollection<T>(
        Func<IEnumerable<T>> getSource,
        Func<T, int, BindingObject<T>>? createBindingSetting = null,
        Func<T, T, bool>? equals = null) {
        var prevValues = new ObservableCollection<BindingObject<T>>();
        equals ??= ((a, b) => EqualityComparer<T>.Default.Equals(a, b));
        var emptyDic = new Dictionary<string, IBindableValueRenderer<T>>();
        createBindingSetting ??= (x, _) => new BindingObject<T>(x, emptyDic);
        return () => {
            var endIndex = ApplyToBindingObject(prevValues, createBindingSetting, getSource());
            // 配列の要素の移動を最小化するために後ろから削除する
            for (int i = prevValues.Count - 1; endIndex <= i; i--) {
                prevValues.RemoveAt(i);
            }
            return prevValues;
        };
    }
}

public class RenderView<TState> : ComponentBase {
    readonly BindingObject<TState> bindingObject;
    readonly INameScope nameScope;
    readonly Func<BindingObject<TState>, INameScope, Control> build;

    public RenderView(
        BindingObject<TState> bindingObject,
        INameScope nameScope,
        Func<BindingObject<TState>, INameScope, Control> build) : base(true) {
        this.bindingObject = bindingObject;
        this.build = build;
        this.nameScope = nameScope;
        OnCreatedCore();
        Initialize();
    }

    protected override object Build() => build(bindingObject, nameScope);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        bindingObject.OnFinishRender += StateHasChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        bindingObject.OnFinishRender -= StateHasChanged;
    }
}

public static class RenderView {
    public static FuncDataTemplate<BindingObject<T>> CreateDataTemplate<T>(Func<BindingObject<T>, INameScope, Control> build) {
        return new FuncDataTemplate<BindingObject<T>>((data, scope) => new RenderView<T>(data, scope, build));
    }
}

public class CircuitDesignerView : ComponentBase, IRender<CircuitDesignerModel> {

    IDispatcher<ICircuitDesignerMessage> dispatcher;

    public CircuitDesignerView(IDispatcher<ICircuitDesignerMessage> dispatcher) : base(true) {
        this.dispatcher = dispatcher;
        OnCreatedCore();
        Initialize();
    }

    VisualBrush GridDrawingBrush(NodifyEditor editor) => new VisualBrush()
        .TileMode(TileMode.Tile)
        .DestinationRect(new RelativeRect(0, 0, 30, 30, RelativeUnit.Absolute))
        .SourceRect(0, 0, 30, 30)
        .Transform(editor.DpiScaledViewportTransform)
        .Visual(new Rectangle()
            .Width(1)
            .Height(1)
            .Fill(Brushes.White));

    bool isDec = false;

    CircuitDesignerModel state = CircuitDesignerModel.Default;

    protected override object Build() =>
        new Grid()
        .Background(() => new SolidColorBrush(Color.FromRgb(0xA, 0x17, 0x2A)))
        .Children([
            new NodifyEditor()
            .GridCellSize(10)
            .Ref(out var nodifyEditor)
            .Background(GridDrawingBrush(nodifyEditor))
            .Connections(BindingExtensions.AsBindableObservableCollection(() => state.Connections))
            .ConnectionTemplate(RenderView.CreateDataTemplate<ConnectionID>((connection, _) =>
                new LineConnection()
                .Source(() => state.GetConnectorAnchor(connection.State.Source), x => { Console.WriteLine($"Connection Source {x}"); })
                .Target(() => state.GetConnectorAnchor(connection.State.Target), x => { Console.WriteLine($"Connection Target {x}"); })
                )
                .ToDataTemplate())
            .ConnectionCompletedCommand(new DelegateCommand<(object? source, object? target)>(t => {
                var source = (BindingObject<ConnectorModel>?)t.source;
                var target = (BindingObject<ConnectorModel>?)t.target;
                if (source == null || target == null) {
                    return;
                }
                // 常にOutからInの方向に接続を行うようにする
                if (source.State.ID.Dir == ConnectionDir.Input && target.State.ID.Dir == ConnectionDir.Output) {
                    dispatcher.Dispatch(new OnConnectNodeMessage(new ConnectionID(Source: target.State.ID, Target: source.State.ID)));
                }
                else if (source.State.ID.Dir == ConnectionDir.Output && target.State.ID.Dir == ConnectionDir.Input) {
                    dispatcher.Dispatch(new OnConnectNodeMessage(new ConnectionID(Source: source.State.ID, Target: target.State.ID)));
                }
            }))
            .DisconnectConnectorCommand(new DelegateCommand<BindingObject<ConnectorModel>>(connector => {
                dispatcher.Dispatch(new OnDisconnectNodeMessage(connector.State.ID));
            }))
            .ItemContainerTheme(new ControlTheme(typeof(ItemContainer)) {
                Setters = {
                    new Setter(
                        ItemContainer.LocationProperty,
                        new Binding($"Bindables[Location].Value"))
                }
            })
            .ItemsSource(BindingExtensions.AsBindableObservableCollection(
                () => state.Nodes.Values,
                (x, _) => new BindingObject<NodeModel>(x, new Dictionary<string, IBindableValueRenderer<NodeModel>> {
                    { nameof(NodeModel.Location),
                        new BindableValue<NodeModel, Point>(new Point(0, 0),
                            x => x.Location,
                            newPos => dispatcher.Dispatch(new OnChangedNodeLocationMessage(x.NodeID, newPos))) }
                })))
            .DataTemplates(RenderView.CreateDataTemplate<NodeModel>((value, _) => {
                var and = new AndView();
                var or = new OrView();
                var not = new NotView();
                var nand = new NandView();
                var nor = new NorView();
                var xor = new XorView();
                return new Node()
                .Header(() => value.State.Title)
                .Input(BindingExtensions.AsBindableObservableCollection(() => value.State.Input))
                .Output(BindingExtensions.AsBindableObservableCollection(() => value.State.Output))
                .Content(() => value.State.Logic switch {
                    LogicKind.And => and,
                    LogicKind.Or => or,
                    LogicKind.Not => not,
                    LogicKind.Nand => nand,
                    LogicKind.Nor => nor,
                    LogicKind.Xor => xor,
                    _ => ""
                })
                .InputConnectorTemplate(RenderView.CreateDataTemplate<ConnectorModel>((connector, _) =>
                    new NodeInput()
                    .Header(() => connector.State.Title)
                    .IsConnected(() => connector.State.IsConnected)
                    .Anchor(() => connector.State.Anchor, x => {
                        Console.WriteLine($"Input {x}");
                        dispatcher.Dispatch(new OnChangedNodeAnchorMessage(connector.State.ID, x));
                    }))
                    .ToDataTemplate())
                .OutputConnectorTemplate(RenderView.CreateDataTemplate<ConnectorModel>((connector, _) =>
                    new NodeOutput()
                    .Header(() => connector.State.Title)
                    .IsConnected(() => connector.State.IsConnected)
                    .Anchor(() => connector.State.Anchor, x => {
                        Console.WriteLine($"Output {x}");
                        dispatcher.Dispatch(new OnChangedNodeAnchorMessage(connector.State.ID, x));
                    }))
                    .ToDataTemplate());
                })),
        new StackPanel()
            .Height(100)
            .Width(200)
            .Background(() => Brushes.White)
            .Children([
                new CheckBox()
                    .IsChecked(() => isDec, isChecked => isDec = isChecked ?? false),
                new Button()
                    .Content("add node")
                    .OnClick(e => dispatcher.Dispatch(Singleton<OnAddNodeMessage>.Instance))
                ])
    ]);

    public void Render(CircuitDesignerModel state) {
        this.state = state;
        StateHasChanged();
    }
}
