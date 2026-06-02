#import <AVFoundation/AVFoundation.h>

extern "C" {
    void RestoreAudioSession()
    {
        NSError *error = nil;
        AVAudioSession *session = [AVAudioSession sharedInstance];
        
        [session setCategory:AVAudioSessionCategoryPlayback error:&error];
        [session setActive:YES error:&error];

        if (error != nil)
        {
            NSLog(@"[AudioSessionFixer] Error restoring audio session: %@", error.localizedDescription);
        }
        else
        {
            NSLog(@"[AudioSessionFixer] Audio session successfully restored.");
        }
    }
}
