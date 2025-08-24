#ifndef I_LOGGER_H
#define I_LOGGER_H

#include <stdarg.h>
#include <stddef.h>

typedef enum LogLevel_DECL {
    LogLevel_Trace,
    LogLevel_Debug,
    LogLevel_Info,
    LogLevel_Warn,
    LogLevel_Error
} LogLevel;

typedef struct ILogger_DECL {
    struct ILogger_Methods_DECL* methods;
} ILogger;

typedef struct ILogger_Methods_DECL {
    void (*vprintf)(ILogger* self, const char* format, va_list* args);
} ILogger_Methods;

static inline void ILogger_printf(ILogger* self, const char* format, ...) {
    va_list args;
    va_start(args, format);
    //ILogger_vprintf(self, format, &args);
    va_end(args);
}

static inline void ILogger_vprintf(ILogger* self, const char* format, va_list* args) {
    if (self == NULL || self->methods == NULL || self->methods->vprintf == NULL) {
        return;
    }
    self->methods->vprintf(self, format, args);
}

#endif
