Imports System.IO
Imports System.Runtime.InteropServices
Imports EasyHookLib

Module Core
    Sub Main()
        Dim targetPID As Int32 = 0
        Dim targetExe As String = Nothing
        Dim channelName As String = Nothing
        ProcessArgs(Environment.GetCommandLineArgs(), targetPID, targetExe)
        If targetPID <= 0 AndAlso String.IsNullOrEmpty(targetExe) Then Return
        EasyHook.RemoteHooking.IpcCreateServer(Of Server)(channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton)
        Dim injectionLibrary As String = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "EasyHookLib.dll")

        Try

            If targetPID > 0 Then
                Console.WriteLine("Attempting to inject into process {0}", targetPID)
                EasyHook.RemoteHooking.Inject(targetPID, injectionLibrary, injectionLibrary, channelName)
            ElseIf Not String.IsNullOrEmpty(targetExe) Then
                Console.WriteLine("Attempting to create and inject into {0}", targetExe)
                EasyHook.RemoteHooking.CreateAndInject(targetExe, "", 0, EasyHook.InjectionOptions.DoNotRequireStrongName, injectionLibrary, injectionLibrary, targetPID, channelName)
            End If

        Catch e As Exception
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine("There was an error while injecting into target:")
            Console.ResetColor()
            Console.WriteLine(e.ToString())
        End Try

        Console.ForegroundColor = ConsoleColor.DarkGreen
        Console.WriteLine("<Press any key to exit>")
        Console.ResetColor()
        Console.ReadKey()
    End Sub

    Private Sub ProcessArgs(ByVal args As String(), <Out> ByRef targetPID As Integer, <Out> ByRef targetExe As String)
        targetPID = 0
        targetExe = Nothing

        While (args.Length <> 2) OrElse Not Int32.TryParse(args(1), targetPID)
            If targetPID > 0 Then
                Exit While
            End If

            If args.Length <> 2 Then
                Console.WriteLine("Usage: FileMonitor ProcessID")
                Console.WriteLine("")
                Console.WriteLine("e.g. : FileMonitor 1234")
                Console.WriteLine("          to monitor an existing process with PID 1234")
                Console.WriteLine()
                Console.WriteLine("Enter a process Id")
                Console.Write("> ")
                args = New String() {args(0), Console.ReadLine()}
                If String.IsNullOrEmpty(args(1)) Then Return
            Else
                targetExe = args(1)
                Exit While
            End If
        End While
    End Sub
End Module
