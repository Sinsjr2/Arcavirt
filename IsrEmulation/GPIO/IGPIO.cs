namespace GPIO; 

public interface IGPIO {
    bool GPIOValue { get; set; }
    event Action OnChangedGPIOValue;
}
