Public Class Server
    Inherits MarshalByRefObject

    Public Sub IsInstalled(ByVal clientPID As Integer)
        Console.WriteLine("FileMonitor has injected FileMonitorHook into process {0}." & vbCrLf, clientPID)
    End Sub

    Public Sub ReportMessages(ByVal clientPID As Integer, ByVal messages As String())
        For i As Integer = 0 To messages.Length - 1
            Console.WriteLine(messages(i))
        Next
    End Sub

    Public Sub ReportMessage(ByVal clientPID As Integer, ByVal message As String)
        Console.WriteLine(message)
    End Sub

    Public Sub ReportException(ByVal e As Exception)
        Console.WriteLine("The target process has reported an error:" & vbCrLf & e.ToString())
    End Sub

    Public Sub Ping()
    End Sub
End Class
