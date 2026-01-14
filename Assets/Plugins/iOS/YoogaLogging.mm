#import <Foundation/Foundation.h>

__attribute__((visibility("default")))
extern "C" bool Yooga_IsLoggingEnabledFromPlist()
{
    id val = [[NSBundle mainBundle] objectForInfoDictionaryKey:@"YOOGA_LOGGING"];
    if (!val) return false;

    if ([val isKindOfClass:[NSNumber class]])
        return [(NSNumber *)val boolValue];

    if ([val isKindOfClass:[NSString class]])
    {
        NSString *s = [(NSString *)val lowercaseString];
        return [s isEqualToString:@"1"] ||
               [s isEqualToString:@"true"] ||
               [s isEqualToString:@"yes"];
    }

    return false;
}
