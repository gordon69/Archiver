# Archiver
To run this project, you need to build dll's from C++ "HUFF" and "LZSS",
solution configuration - Release and solution platform - Win32(x86).
Then build C#, solution configuration - Debug or Release and solution platform - Win32(x86),
and copy "HUFF.dll" and "LZSS.dll" to directory
where is ".exe" file from C# project (it should be in "...\bin\x86\Release" or "...\bin\x86\Debug").
After this you need to create the "tmp" folder in the same directory to avoid bugs.
After all these steps, you can start debugging the application.
