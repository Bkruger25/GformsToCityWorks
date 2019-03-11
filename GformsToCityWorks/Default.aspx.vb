Imports System.Data
Imports System.Data.Sql
Imports System.Data.SqlClient
Imports System.IO
Imports Ionic.Zip


Public Class _Default
    Inherits Page

    Dim Conn As New SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings("db").ConnectionString)
    Private Property cmd As SqlCommand
    Private Property cmd2 As SqlCommand
    Private Property cmd3 As SqlCommand
    Private Property cmd4 As SqlCommand
    Dim rdr As SqlDataReader
    Dim rdr2 As SqlDataReader
    Dim rdr3 As SqlDataReader
    Dim rdr4 As SqlDataReader

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

    End Sub

    Protected Sub btnSync_Click(sender As Object, e As EventArgs) Handles btnSync.Click
        Dim cwIDQuery As String
        Dim tblName As String
        Dim cwID As String
        Dim txtString As String
        Dim counter As Integer
        Dim strErr As String

        tblName = UCase(txtFormName.Text)

        System.IO.File.WriteAllText(Server.MapPath("~\TextFile\log.txt"), "")
        System.IO.File.WriteAllText(Server.MapPath("~\TextFile\error.txt"), "")

        'get city works id
        counter = 1
        Conn.Open()
        cwIDQuery = "Select [CITYWORKS_ID] from [Centlec_Inspections].[sde].[TBL_" & tblName & "] where PK_" & tblName & "_ID = (select max(PK_" & tblName & "_ID) from SDE.TBL_" & tblName & ")"
        Using cmd As New SqlCommand
            With cmd
                .Connection = Conn
                .CommandType = Data.CommandType.Text
                .CommandText = cwIDQuery
            End With
            rdr = cmd.ExecuteReader
            If rdr.HasRows Then
                Do While rdr.Read
                    cwID = rdr.Item("CITYWORKS_ID")
                Loop
            End If
            rdr.Close()
        End Using


        If cwID = 0 Then
            Exit Sub
        Else
            'get i-ID, q-ID, q-seq, question
            Dim questionTable As String
            questionTable = "select INSPECTIONID, QUESTIONID, QUESTIONSEQUENCE, QUESTION from [CW_GISCOE].[azteca].[INSPQUESTION] where INSPECTIONID = '" & cwID & "'"
            Using cmd2 As New SqlCommand
                With cmd2
                    .Connection = Conn
                    .CommandType = Data.CommandType.Text
                    .CommandText = questionTable
                End With
                rdr2 = cmd2.ExecuteReader

                Dim inspectionID As String
                Dim questionID As String
                Dim questionseq As String
                Dim question As String

                If rdr2.HasRows Then
                    While rdr2.Read
                        inspectionID = rdr2.Item(0)
                        questionID = rdr2.Item(1)
                        questionseq = rdr2.Item(2)
                        question = rdr2.Item(3)

                        'get inspectionID 
                        'Primary table or sub table
                        Dim inspectionquery As String
                        Dim QinspectionID As String
                        inspectionquery = "select max(PK_" & tblName & "_ID) from sde.TBL_" & tblName
                        Using cmd3 As New SqlCommand
                            With cmd3
                                .Connection = Conn
                                .CommandType = Data.CommandType.Text
                                .CommandText = inspectionquery
                            End With
                            rdr3 = cmd3.ExecuteReader
                            If rdr3.HasRows Then
                                Do While rdr3.Read
                                    QinspectionID = rdr3.Item(0)
                                Loop
                            End If

                            'get answer
                            Dim answerQuery As String
                            Dim answer As String

                            answerQuery = "select " & question & " from sde.TBL_" & tblName & " where PK_" & tblName & "_ID = " & QinspectionID
                            Using cmd4 As New SqlCommand
                                With cmd4
                                    .Connection = Conn
                                    .CommandType = Data.CommandType.Text
                                    .CommandText = answerQuery
                                End With
                                Try
                                    rdr4 = cmd4.ExecuteReader
                                    Do While rdr4.Read
                                        answer = rdr4.Item(0)
                                    Loop
                                Catch ex As Exception
                                    strErr = strErr & " " & ex.Message & vbCrLf

                                End Try



                                txtString = "---------" & question & "---------" & vbCrLf &
                                    "declare @ABC" & question & " as varchar(100) = (select " & question & " from sde.TBL_" & tblName & " where PK_" & tblName & "_ID = @ABC_InspectionID)" &
                                    "declare @" & question & "1 as int = (SELECT [INSPQUESTIONID] FROM [CW_GISCOE].[azteca].[INSPQUESTION] where INSPECTIONID = " & vbCrLf &
                                    cwID & " and question = '" & question & "')" & vbCrLf &
                                    "declare @p" & counter & "int " & vbCrLf &
                                    "set @p" & counter & "=(select max(INSPQUESTIONID) +1 from [CW_GISCOE].[azteca].[INSPQUESTION])" & vbCrLf &
                                    "IF @" & question & "1 is not null" & vbCrLf &
                                    "exec CW_GISCOE.azteca.InspQuestion_Update" & vbCrLf &
                                    "@InspecQuestionID=@" & question & "1, @InspectionId=" & cwID & ",@Question=N'" & question & "',@QuestionID=" & questionID & ",@QuestionSequence=" & questionseq & ",@Answer=" & answer & ",@Instruction=N'',@Explanation=N'',@Score=0,@NumericAnswer=0" & vbCrLf &
                                    "ELSE BEGIN" & vbCrLf &
                                    "exec CW_GISCOE.azteca.InspQuestion_Create @InspQuestionId=@p" & counter & ",,@InspQuestionIdInput=0,@InspectionId=" & cwID & ",@Question=N'" & questionseq & "',@QuestionId=" & questionID & ",@QuestionSequence=" & questionseq & ",@Answer=" & answer & ",@Instruction=N'',@Explanation=N'',@Weight=0,@Score=0,@NumericAnswer=0" & vbCrLf &
                                    "END;"

                                Using writer As New StreamWriter(Server.MapPath("~\TextFile\log.txt"), True)
                                    writer.WriteLine(txtString)
                                    writer.WriteLine("")
                                    'writer.WriteLine("Important data line 2")
                                End Using

                            End Using
                        End Using
                    End While
                End If
            End Using

        End If
        Conn.Close()
        Using writer As New StreamWriter(Server.MapPath("~\TextFile\error.txt"), True)
            writer.WriteLine(strErr)
            writer.WriteLine("")
            'writer.WriteLine("Important data line 2")
        End Using
        'write txt file back
        Using zip As New ZipFile()
            zip.AlternateEncodingUsage = ZipOption.AsNecessary
            zip.AddDirectoryByName("Files")
            zip.AddFile((Server.MapPath("~\TextFile\log.txt")), "Files")
            zip.AddFile((Server.MapPath("~\TextFile\error.txt")), "Files")
            Response.Clear()
            Response.BufferOutput = False
            Dim zipName As String = [String].Format("Zip_{0}.zip", DateTime.Now.ToString("yyyy-MMM-dd-HHmmss"))
            Response.ContentType = "application/zip"
            Response.AddHeader("content-disposition", "attachment; filename=" + zipName)
            zip.Save(Response.OutputStream)
            Response.End()
        End Using


        'Response.AppendHeader("Content-Disposition", "attachment; filename=log.txt")
        'Response.ContentType = "text"
        'Response.TransmitFile((Server.MapPath("~\TextFile\log.txt")))

        'Response.[End]()
    End Sub
End Class