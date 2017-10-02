Imports System.Data.SqlClient
Public Class GameForm

    Dim dsQuestions As New HangmanDBDataSet()   'Dataset containing all the words and word hints in two tables
    Dim numWord As Integer = -1                 'The number or the word's database entry
    Dim strWord As String                       'The current word being guessed
    Dim numRightGuesses As Integer              'The number of letters in the strWord correctly guessed so far
    Dim numWrongGuesses As Integer              'The number of letters guessed that are NOT in strWord (6 wrong guesses and you lose)
    Dim numTotalGuesses As Integer              'The number of total guesses, because one is needed to reflect correct guesses not matches
    Dim blnGameStarted As Boolean               'Boolean to tell program if a game is in progress or not
    Dim CurWordLength As Integer                'The length of the current strWord being guessed
    Dim CurWordCharArr As Char()                'The current strWord being guessed, converted to all uppercase and converted into a Char Array
    Dim CurLetterGuess As Char                  'The Current letter being guessed converted into a single Char

    Private Sub GameForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        'Disable the textbox for guessing letters
        txtGuess.Enabled = False
        'Reset all of the other controls on the Game board
        ResetAll()
        'Fill the Dataset from the two tables in the Database
        Dim connstr As String = "Data Source=.\SQLEXPRESS;AttachDbFilename=|DataDirectory|\HangmanDB.mdf;Integrated Security=True;User Instance=True"
        Dim conn As New SqlConnection(connstr)
        Dim str1 As String = "SELECT ID, Description FROM Units"
        Dim cmd As New SqlCommand(str1, conn)
        Dim daUnits As New SqlDataAdapter(cmd)
        'fill the Dataset with the Units table
        daUnits.Fill(dsQuestions, "Units")
        Dim str2 As String = "SELECT WordID, Word, UnitID FROM dbo.Words"
        Dim cmd2 As New SqlCommand(str2, conn)
        Dim daWords As New SqlDataAdapter(cmd2)
        'fill the dataset with the Words table
        daWords.Fill(dsQuestions, "Words")
        'initialize the boolean for a new game to false
        blnGameStarted = False
    End Sub
    Private Sub NewToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles NewToolStripMenuItem.Click
        'start a new game
        blnGameStarted = True
        'go to the next word in the Dataset
        numWord += 1
        'reset the right and wrong guess counters
        numRightGuesses = 0
        numWrongGuesses = 0
        numTotalGuesses = 0
        'Enabled the guess letter textbox and give it the focus
        txtGuess.Enabled = True
        txtGuess.Clear()
        txtGuess.Focus()
        'show an empty hangman gallows
        pBoxHangman.ImageLocation = "Images\Hangman-0.png"
        'clear all the letters from the previous game
        ClearLetters()
        'load the next word
        strWord = CStr(dsQuestions.Words(numWord)(1))
        'load the next hint
        lblUnitDesc.Text = CStr(dsQuestions.Words(numWord).UnitsRow(1))
        'set the blank labels for the word in the panel1 control
        For x As Integer = 0 To strWord.Length - 1
            Panel1.Controls.Item(x).Text = "_"
        Next
    End Sub
    Private Sub QuitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles QuitToolStripMenuItem.Click
        Application.Exit()
    End Sub
    Private Sub ResetAll()
        'clear all text, labels and pictures
        ClearLetters()
        lblUnitDesc.Text = ""
        pBoxHangman.ImageLocation = ""
        txtGuess.Text = ""
    End Sub
    Private Sub ClearLetters()
        'clear all the letters for a new game.
        'remember, the labels for the letters are part of the Panel1.Controls collection,
        'so we can loop through them like any other collection.
        For Each lbl As Label In Panel1.Controls
            lbl.Text = ""
        Next
    End Sub
    Private Sub txtGuess_TextChanged(sender As Object, e As EventArgs) Handles txtGuess.TextChanged
        'First see if a new game has bee started
        If blnGameStarted = True Then
            If txtGuess.Text <> "" Then  '.Trim
                'make input string in textbox a Char Array with one member and convert it to upper case
                Dim chrArrGuess As Char() = txtGuess.Text.ToUpper.ToCharArray  'Trim.
                'Set the current letter guessed to the first letter value of the Guessed letter textbox
                CurLetterGuess = chrArrGuess(0)
                'convert guessed letter to ascii character 
                Dim AsciiGuess As Integer = Asc(CurLetterGuess)
                'see if it is an uppercase letter of the alphabet
                If AsciiGuess > 64 And AsciiGuess < 91 Then
                    'character is an uppercase letter of the alpabet, so compare it to the strWord converted to Uppercase
                    numTotalGuesses += 1
                    checkProgress(strWord.ToUpper(), CurLetterGuess)
                Else
                    'let the user know that they didn't guess a letter
                    MessageBox.Show("Please select a letter of the Alphabet", _
                            "Not an Alpabetic character", _
                            MessageBoxButtons.OK, _
                            MessageBoxIcon.Warning)
                End If
            End If
            txtGuess.Focus()
            txtGuess.SelectAll()
        End If
    End Sub
    Private Sub checkProgress(ByVal Uword As String, ByVal ChrGuess As Char)
        'See if the letter Guessed is in the strWord
        If Uword.Contains(CurLetterGuess.ToString) Then
            'If the letter Guessed is in the strWord then insert it into every position on the hangman board 
            'that it matches the strWord. First, cast the strWord to a Char Array so we can iterate through it
            Dim ChrArrUword As Char() = Uword.ToCharArray
            'now compare the guessed letter to each letter of strWord and fill them into the _ spaces on the board 
            For i As Integer = 0 To Uword.Length - 1
                If ChrArrUword(i) = ChrGuess Then
                    Panel1.Controls.Item(i).Text = ChrGuess.ToString
                    'and add to the  numRightGuesses counter
                    numRightGuesses += 1
                End If
            Next
            'Now check to see if they won the game
            If WonGame() = True Then
                WinningMsg()
            Else
                txtGuess.Focus()
                txtGuess.SelectAll()
            End If
        Else
            'or else add it to the numWrongGuesses and see if the player lost the game
            numWrongGuesses += 1
            pBoxHangman.ImageLocation = "Images\Hangman-" & numWrongGuesses & ".png"
            If LostGame() = True Then
                LosingMsg()
            Else
                txtGuess.Focus()
                txtGuess.SelectAll()
            End If
        End If
    End Sub
    Private Function WonGame() As Boolean
        If numRightGuesses = strWord.Length Then
            Return True
        Else
            Return False
        End If
    End Function
    Private Function LostGame() As Boolean
        If numWrongGuesses = 6 Then
            Return True
        Else
            Return False
        End If
    End Function
    Private Sub WinningMsg()
        If MessageBox.Show("You WON in " & numTotalGuesses.ToString & _
                           " tries!" & Chr(10) & "Do you want to play another game?" _
                           , "                YOU WON!", _
                           MessageBoxButtons.YesNo, _
                           MessageBoxIcon.Information) = Windows.Forms.DialogResult.Yes Then
            NewToolStripMenuItem_Click(Me, New System.EventArgs)
        Else
            Application.Exit()
        End If
    End Sub
    Private Sub LosingMsg()
        If MessageBox.Show("You Lost in " & numTotalGuesses.ToString & _
                           " tries. The word was " & strWord & "." & Chr(10) & _
                           "Do you want to play another game?" _
                           , "                YOU LOST!", _
                           MessageBoxButtons.YesNo, _
                           MessageBoxIcon.Information) = Windows.Forms.DialogResult.Yes Then
            NewToolStripMenuItem_Click(Me, New System.EventArgs)
        Else
            Application.Exit()
        End If
    End Sub
End Class
