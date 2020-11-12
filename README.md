This is the old AutoHotKey (AHK) version of WinCompose. WinCompose was rewritten
to C# and WPF in 2014 for multiple reasons:

 - C# is overall a better language; AHK has bizzare syntax, mixes declarative and
   imperative paradigms, is full of inconsistencies (case sensitivity of != _vs_ ==,
   hash tables are case-insensitive),
   lacks proper threading support
 - AHK is very slow in general and did not cope well with having to execute code at
   each keypress
 - AHK does not understand keyboard layout differences between windows or layout changes
   in general; I had to maintain a custom fork in order to handle those
 - GUI features of AHK are very limited: no responsive layout, no virtualised controls,
   no Emoji support, poor rendering performance
