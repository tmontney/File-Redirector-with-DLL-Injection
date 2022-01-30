Imports System.Runtime.InteropServices
Imports System.Text

Public Class FileRedirector
    Implements EasyHook.IEntryPoint

    Private _server As Server = Nothing
    Private _messageQueue As Queue(Of String) = New Queue(Of String)()
    Private redirectFilenameFrom As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\from.txt"
    Private redirectFilenameTo As String = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\to.txt"

    Public Sub New(ByVal context As EasyHook.RemoteHooking.IContext, ByVal channelName As String)
        _server = EasyHook.RemoteHooking.IpcConnectClient(Of Server)(channelName)
        _server.Ping()
    End Sub

    Public Sub Run(ByVal context As EasyHook.RemoteHooking.IContext, ByVal channelName As String)
        _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId())
        Dim createFileHook = EasyHook.LocalHook.Create(EasyHook.LocalHook.GetProcAddress("kernel32.dll", "CreateFileW"), New CreateFile_Delegate(AddressOf CreateFile_Hook), Me)
        createFileHook.ThreadACL.SetExclusiveACL(New Int32() {0})
        _server.ReportMessage(0, "CreateFile hook installed")
        EasyHook.RemoteHooking.WakeUpProcess()

        Try

            While True
                System.Threading.Thread.Sleep(500)
                Dim queued As String() = Nothing

                SyncLock _messageQueue
                    queued = _messageQueue.ToArray()
                    _messageQueue.Clear()
                End SyncLock

                If queued IsNot Nothing AndAlso queued.Length > 0 Then
                    _server.ReportMessages(0, queued)
                Else
                    _server.Ping()
                End If
            End While

        Catch
        End Try

        createFileHook.Dispose()
        EasyHook.LocalHook.Release()
    End Sub

    <DllImport("Kernel32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Private Shared Function GetFinalPathNameByHandle(ByVal hFile As IntPtr,
    <MarshalAs(UnmanagedType.LPTStr)> ByVal lpszFilePath As StringBuilder, ByVal cchFilePath As UInteger, ByVal dwFlags As UInteger) As UInteger
    End Function
    <UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet:=CharSet.Unicode, SetLastError:=True)>
    Delegate Function CreateFile_Delegate(ByVal filename As String, ByVal desiredAccess As UInt32, ByVal shareMode As UInt32, ByVal securityAttributes As IntPtr, ByVal creationDisposition As UInt32, ByVal flagsAndAttributes As UInt32, ByVal templateFile As IntPtr) As IntPtr
    <DllImport("kernel32.dll", CharSet:=CharSet.Unicode, SetLastError:=True, CallingConvention:=CallingConvention.StdCall)>
    Private Shared Function CreateFileW(ByVal filename As String, ByVal desiredAccess As UInt32, ByVal shareMode As UInt32, ByVal securityAttributes As IntPtr, ByVal creationDisposition As UInt32, ByVal flagsAndAttributes As UInt32, ByVal templateFile As IntPtr) As IntPtr
    End Function

    Private Function CreateFile_Hook(ByVal filename As String, ByVal desiredAccess As UInt32, ByVal shareMode As UInt32, ByVal securityAttributes As IntPtr, ByVal creationDisposition As UInt32, ByVal flagsAndAttributes As UInt32, ByVal templateFile As IntPtr) As IntPtr
        Try
            SyncLock Me._messageQueue

                If Me._messageQueue.Count < 1000 Then
                    Dim mode As String = String.Empty

                    Select Case creationDisposition
                        Case 1
                            mode = "CREATE_NEW"
                        Case 2
                            mode = "CREATE_ALWAYS"
                        Case 3
                            mode = "OPEN_ALWAYS"
                        Case 4
                            mode = "OPEN_EXISTING"
                        Case 5
                            mode = "TRUNCATE_EXISTING"
                    End Select

                    Me._messageQueue.Enqueue(String.Format("[{0}:{1}]: CREATE ({2}) ""{3}""", EasyHook.RemoteHooking.GetCurrentProcessId(), EasyHook.RemoteHooking.GetCurrentThreadId(), mode, filename))
                End If
            End SyncLock

        Catch
        End Try

        If filename = redirectFilenameFrom Then
            filename = redirectFilenameTo
            Me._messageQueue.Enqueue(String.Format("Redirecting: {0} --> {1} ", redirectFilenameFrom, redirectFilenameTo))
        End If

        Return CreateFileW(filename, desiredAccess, shareMode, securityAttributes, creationDisposition, flagsAndAttributes, templateFile)
    End Function
End Class
