Imports System.IO
Imports System.Threading
Module Module1


    'The greaterest VB roguelike that ever did a making of for great good!
    'Written by Samuel Kinnett, 2013

    Dim SburbFolderLocation As String       'The location of the SBURB folder


    Dim BinaryWriter As BinaryWriter        'Used to write to binary files
    Dim BinaryReader As BinaryReader        'Used to read from binary files
    Dim TextReader As StreamReader          'Used to read in text files
    Dim TextWriter As StreamWriter          'Used to write to text files
    Dim CurrentFile As FileStream           'The file being accessed

    Dim WorldName1 As String
    Dim WorldName2 As String                'land of [WORLDNAME1] and [WORLDNAME2]'

    Dim SolidMap(63, 39) As Integer         'this map consists of 1's and 0's. A 1 indicates solid space e.g. a wall, and a 0 indicates empty space that the player can walk through
    Dim TerrainMap(63, 39) As Integer       'this map consists of a variety of numbers indicating the material of the 'block' in that space, e.g. stone or wood.
    Dim ItemMap(63, 39) As String           'this map contains the location of items in the world. Multiple items in one square are divided by a semicolon, e.g. "WOODEN SWORD:WOODEN SHIELD"
    Dim VisibilityMap(63, 39) As Integer    'this array contains a map showing the currently visible, and previously explored, areas of the map.
    Dim Entitymap(63, 39) As Integer        'this array contains the locations of all of the entities in the level.

    Dim MapIndex(,) As Integer              'this stores all of the currently generated dungeons locations in the file system. e.g. ([Dungeon Number],[File Number])
    Dim CurrentMapName As String            'this is the name of the current map that will be saved along with the map data. Randomly generated from an array of names.
    Dim MapNameBank(4, 4) As String         'all the potential name segments for a map. Divided into 4 sections.
    Dim MapArchetype As Integer             'the type of dungeon being generated, e.g. 0 = traditional stony cavey dungeon, 1 = ice dungeon etc.

    Dim EventLog() As String                'stores all 'EventBox' output for future perusal
    Dim EventlogSize As Integer             'The size of the Event log

    Dim RoomCentres(,) As Integer           'stores the centres of each room for the purposes of linking them via corridors.
    Dim UpStair(1) As Integer               'the X and Y coordinates of the Upwards staircase
    Dim DownStair(1) As Integer             'the X and Y coordinates of the downwards staircase

    Sub Main()

        Console.WindowWidth = 80
        Console.WindowHeight = 50

        Call RenderLoadMenu()
        Console.ReadLine()

        My.Computer.Audio.Play(My.Resources._04_Sburban_Jungle__Brief_Mix_, AudioPlayMode.Background)
        Call LoadData()
    End Sub

    Sub RenderLoadMenu()
        Console.SetCursorPosition(0, 0)
        Console.ForegroundColor = ConsoleColor.Green
        Console.Clear()
        Console.WriteLine()
        Console.WriteLine()
        Console.WriteLine("             █")
        Console.WriteLine("             ██")
        Console.WriteLine("      ████  ████")
        Console.WriteLine("      ████ ██████")
        Console.WriteLine("      █████████████")
        Console.WriteLine("      ███████████████")
        Console.WriteLine("     ██████████████████")
        Console.WriteLine("    █████████████████████")
        Console.WriteLine("   ███████████████████████")
        Console.WriteLine("  ████████████")
        Console.WriteLine("               ████████")
        Console.WriteLine("     ████████  ████████                  SBURB V0.01")
        Console.WriteLine("     ████████  ████████")
        Console.WriteLine("     ████████  ████████               (C) SKAIANET 2013")
        Console.WriteLine("     ████████  ████████")
        Console.WriteLine("     ████████     █████")
        Console.WriteLine("     ████████ ███ █████")
        Console.WriteLine("     ████████ ███ █████")
        Console.WriteLine("     ████████ ███ █████")
        Console.WriteLine()
        Console.WriteLine("     ████████ ████████")
        Console.WriteLine("     ████████ ████████")
        Console.WriteLine("     ████████ ████████           PRESS ENTER TO BEGIN LOADING")
        Console.WriteLine("     ████████ ████████")
        Console.WriteLine("     ████████ ████████")
        Console.WriteLine("     ████████ ████████")
        Console.WriteLine("     ████████ ████████")
        Console.WriteLine("     ████████ ████████")

        Console.WriteLine()
        Console.WriteLine("  ┌──────────────────────────────────────────────────────────────────────────┐")
        Console.WriteLine("  │                                                                          │")
        Console.WriteLine("  └──────────────────────────────────────────────────────────────────────────┘")

    End Sub

    Sub LoadData()

        Dim TempString As String                    'Used to store the current event log being accessed. This allows the size of the event log to be calculated.
        Dim ValidDrives() As String                 'All the hard drives on the computer.
        Dim NumOfDrives As Integer                  'The number of drives on the computer
        Dim DirectoryFound As Boolean = False       'Has the SBURB directory been found?
        Dim ChosenDrive As String                   'The drive to save SBURB to
        Dim Key As Integer                          'Variable for keyboard input
        Dim CurrentlySelectedDriveNumber As Integer

        Dim TestString As String = "Top Kek"

        Call GetDrives(ValidDrives, NumOfDrives)

        'Code will go here that checks if the necessary files are in place, and creates them if they aren't. It then loads all the random names into the arrays.

        Console.SetCursorPosition(3, 32)
        Console.Write("▒")
        Console.SetCursorPosition(0, 36)
        Console.WriteLine("                          Checking File Directories                            ")
        Try
            For DriveCount As Integer = 0 To NumOfDrives
                If My.Computer.FileSystem.DirectoryExists(ValidDrives(DriveCount) & ":\SBURB") Then
                    DirectoryFound = True
                    SburbFolderLocation = ValidDrives(DriveCount) & ":\SBURB\"
                    Exit For
                End If
            Next
        Catch ex As Exception
            'Error catching stuff can go here
        End Try
        If DirectoryFound = True Then
            Console.SetCursorPosition(3, 32)
            Console.Write("█")
        Else
            Console.SetCursorPosition(20, 20)
            Console.Write("┌────────────────────────────────────────┐")
            Console.SetCursorPosition(20, 21)
            Console.Write("│ ERROR: SBURB MAIN DIRECTORY NOT FOUND! │")
            Console.SetCursorPosition(20, 22)
            Console.Write("│ SELECT DRIVE TO INSTALL DIRECTORY:     │")
            Console.SetCursorPosition(20, 23)
            Console.Write("│                                        │")
            For X As Integer = 24 To (24 + NumOfDrives)
                Console.SetCursorPosition(20, X)
                Console.Write("│ " & ValidDrives(X - 24) & ":\                                    ")
                Console.SetCursorPosition(61, X)
                Console.WriteLine("│")
                If X = (24 + NumOfDrives) Then
                    Console.SetCursorPosition(20, X + 1)
                    Console.Write("└────────────────────────────────────────┘")
                End If
            Next


            While True
                CurrentlySelectedDriveNumber = 0
                For x As Integer = 24 To (24 + NumOfDrives)
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.ForegroundColor = ConsoleColor.Green
                    Console.SetCursorPosition(22, x)
                    Console.Write(ValidDrives(x - 24) & ":\")
                Next
                Console.BackgroundColor = ConsoleColor.Green
                Console.ForegroundColor = ConsoleColor.Black
                Console.SetCursorPosition(22, 24 + CurrentlySelectedDriveNumber)
                Console.Write(ValidDrives(CurrentlySelectedDriveNumber) & ":\")
                Key = Console.ReadKey(True).Key
                Select Case Key
                    Case 38
                        If CurrentlySelectedDriveNumber <> NumOfDrives Then
                            CurrentlySelectedDriveNumber += 1
                        End If
                    Case 40
                        If CurrentlySelectedDriveNumber <> 0 Then
                            CurrentlySelectedDriveNumber -= 1
                        End If
                    Case 13
                        ChosenDrive = ValidDrives(CurrentlySelectedDriveNumber) & ":\"
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.Green
                        Exit While
                End Select
            End While

            System.Threading.Thread.Sleep(250)
            Try
                My.Computer.FileSystem.CreateDirectory(ChosenDrive & "SBURB")
            Catch ex As Exception
                Call RenderLoadMenu()
                Console.SetCursorPosition(20, 20)
                Console.Write("┌────────────────────────────────────────┐")
                Console.SetCursorPosition(20, 21)
                Console.Write("│       UNABLE TO CREATE DIRECTORY.      │")
                Console.SetCursorPosition(20, 22)
                Console.Write("│                                        │")
                Console.SetCursorPosition(20, 23)
                Console.Write("└────────────────────────────────────────┘")
                Console.ReadLine()
                End
            End Try
            Call RenderLoadMenu()
            Call LoadData()
        End If

        Console.SetCursorPosition(4, 32)
        Console.Write("▒")
        Console.SetCursorPosition(0, 36)
        Console.WriteLine("                   Checking necessary folders are in place                     ")

        Try
            If My.Computer.FileSystem.DirectoryExists(SburbFolderLocation & "Data") Then
                Console.SetCursorPosition(4, 32)
                Console.Write("█")
                Console.SetCursorPosition(5, 32)
                Console.Write("▒")
                If My.Computer.FileSystem.DirectoryExists(SburbFolderLocation & "Data\Maps") Then
                    Console.SetCursorPosition(5, 32)
                    Console.Write("█")
                    Console.SetCursorPosition(6, 32)
                    Console.Write("▒")
                    If My.Computer.FileSystem.DirectoryExists(SburbFolderLocation & "Data\Player") Then
                        Console.SetCursorPosition(6, 32)
                        Console.Write("█")
                        Console.SetCursorPosition(7, 32)
                        Console.Write("▒")
                        If My.Computer.FileSystem.DirectoryExists(SburbFolderLocation & "Data\Items") Then
                            Console.SetCursorPosition(7, 32)
                            Console.Write("█")
                        Else
                            Try
                                Console.SetCursorPosition(20, 20)
                                Console.Write("┌────────────────────────────────────────┐")
                                Console.SetCursorPosition(20, 21)
                                Console.Write("│         CREATING ITEMS FOLDER          │")
                                Console.SetCursorPosition(20, 22)
                                Console.Write("└────────────────────────────────────────┘")
                                System.Threading.Thread.Sleep(500)
                                My.Computer.FileSystem.CreateDirectory(SburbFolderLocation & "Data\Items")
                                Call RenderLoadMenu()
                                Call LoadData()
                            Catch ex As Exception
                                My.Computer.Audio.Stop()
                                Console.SetCursorPosition(20, 20)
                                Console.Write("┌────────────────────────────────────────┐")
                                Console.SetCursorPosition(20, 21)
                                Console.Write("│ ERROR! UNABLE TO CREATE ITEMS FOLDER!  │")
                                Console.SetCursorPosition(20, 22)
                                Console.Write("└────────────────────────────────────────┘")
                                Console.ReadLine()
                                End
                            End Try
                        End If
                    Else
                        Try
                            Console.SetCursorPosition(20, 20)
                            Console.Write("┌────────────────────────────────────────┐")
                            Console.SetCursorPosition(20, 21)
                            Console.Write("│         CREATING PLAYER FOLDER         │")
                            Console.SetCursorPosition(20, 22)
                            Console.Write("└────────────────────────────────────────┘")
                            System.Threading.Thread.Sleep(500)
                            My.Computer.FileSystem.CreateDirectory(SburbFolderLocation & "Data\Player")
                            Call RenderLoadMenu()
                            Call LoadData()
                        Catch ex As Exception
                            My.Computer.Audio.Stop()
                            Console.SetCursorPosition(20, 20)
                            Console.Write("┌────────────────────────────────────────┐")
                            Console.SetCursorPosition(20, 21)
                            Console.Write("│ ERROR! UNABLE TO CREATE PLAYER FOLDER! │")
                            Console.SetCursorPosition(20, 22)
                            Console.Write("└────────────────────────────────────────┘")
                            Console.ReadLine()
                            End
                        End Try
                    End If
                Else
                    Try
                        Console.SetCursorPosition(20, 20)
                        Console.Write("┌────────────────────────────────────────┐")
                        Console.SetCursorPosition(20, 21)
                        Console.Write("│          CREATING MAPS FOLDER          │")
                        Console.SetCursorPosition(20, 22)
                        Console.Write("└────────────────────────────────────────┘")
                        System.Threading.Thread.Sleep(500)
                        My.Computer.FileSystem.CreateDirectory(SburbFolderLocation & "Data\Maps")
                        Call RenderLoadMenu()
                        Call LoadData()
                    Catch ex As Exception
                        My.Computer.Audio.Stop()
                        Console.SetCursorPosition(20, 20)
                        Console.Write("┌────────────────────────────────────────┐")
                        Console.SetCursorPosition(20, 21)
                        Console.Write("│  ERROR! UNABLE TO CREATE MAPS FOLDER!  │")
                        Console.SetCursorPosition(20, 22)
                        Console.Write("└────────────────────────────────────────┘")
                        Console.ReadLine()
                        End
                    End Try
                End If
            Else
                Try
                    Console.SetCursorPosition(20, 20)
                    Console.Write("┌────────────────────────────────────────┐")
                    Console.SetCursorPosition(20, 21)
                    Console.Write("│          CREATING DATA FOLDER          │")
                    Console.SetCursorPosition(20, 22)
                    Console.Write("└────────────────────────────────────────┘")
                    System.Threading.Thread.Sleep(500)
                    My.Computer.FileSystem.CreateDirectory(SburbFolderLocation & "Data")
                    Call RenderLoadMenu()
                    Call LoadData()
                Catch ex As Exception
                    My.Computer.Audio.Stop()
                    Console.SetCursorPosition(20, 20)
                    Console.Write("┌────────────────────────────────────────┐")
                    Console.SetCursorPosition(20, 21)
                    Console.Write("│  ERROR! UNABLE TO CREATE DATA FOLDER!  │")
                    Console.SetCursorPosition(20, 22)
                    Console.Write("└────────────────────────────────────────┘")
                    Console.ReadLine()
                    End
                End Try
            End If
        Catch ex As Exception
            My.Computer.Audio.Stop()
            Console.SetCursorPosition(20, 20)
            Console.Write("┌────────────────────────────────────────┐")
            Console.SetCursorPosition(20, 21)
            Console.Write("│   ERROR: CANNOT ACCESS FILESTRUCTURE!  │")
            Console.SetCursorPosition(20, 22)
            Console.Write("└────────────────────────────────────────┘")
            Console.ReadLine()
            End
        End Try


        Console.SetCursorPosition(8, 32)
        Console.Write("▒")
        Console.SetCursorPosition(0, 36)
        Console.WriteLine("                    Checking necessary files are in place                      ")


        Try
            If My.Computer.FileSystem.FileExists(SburbFolderLocation & "Data\Maps\MapIndex.bin") Then
                Console.WriteLine(SburbFolderLocation & "Data\Maps\MapIndex.bin")
                Console.ReadLine()
                Console.SetCursorPosition(8, 32)
                Console.Write("█")
                Console.SetCursorPosition(9, 32)
                Console.Write("▒")
                If My.Computer.FileSystem.FileExists(SburbFolderLocation & "Data\Player\Config.txt") Then
                    Console.SetCursorPosition(9, 32)
                    Console.Write("█")
                    Console.SetCursorPosition(10, 32)
                    Console.Write("▒")
                Else
                    Try
                        Console.SetCursorPosition(20, 20)
                        Console.Write("┌────────────────────────────────────────┐")
                        Console.SetCursorPosition(20, 21)
                        Console.Write("│          CREATING CONFIG FILE          │")
                        Console.SetCursorPosition(20, 22)
                        Console.Write("└────────────────────────────────────────┘")
                        System.Threading.Thread.Sleep(500)
                        File.Create(SburbFolderLocation & "Data\Player\Config.txt").Dispose()
                        Call RenderLoadMenu()
                        Call LoadData()
                    Catch ex As Exception
                        My.Computer.Audio.Stop()
                        Console.SetCursorPosition(20, 20)
                        Console.Write("┌────────────────────────────────────────┐")
                        Console.SetCursorPosition(20, 21)
                        Console.Write("│  ERROR! UNABLE TO CREATE CONFIG FILE!  │")
                        Console.SetCursorPosition(20, 22)
                        Console.Write("└────────────────────────────────────────┘")
                        Console.ReadLine()
                        End
                    End Try
                End If
            Else
                Try
                    Console.SetCursorPosition(20, 20)
                    Console.Write("┌────────────────────────────────────────┐")
                    Console.SetCursorPosition(20, 21)
                    Console.Write("│           CREATING MAP INDEX           │")
                    Console.SetCursorPosition(20, 22)
                    Console.Write("└────────────────────────────────────────┘")
                    System.Threading.Thread.Sleep(500)
                    File.Create(SburbFolderLocation & "Data\Maps\MapIndex.bin").Dispose()
                    Call RenderLoadMenu()
                    Call LoadData()
                Catch ex As Exception
                    My.Computer.Audio.Stop()
                    Console.SetCursorPosition(20, 20)
                    Console.Write("┌────────────────────────────────────────┐")
                    Console.SetCursorPosition(20, 21)
                    Console.Write("│   ERROR! UNABLE TO CREATE MAP INDEX!   │")
                    Console.SetCursorPosition(20, 22)
                    Console.Write("└────────────────────────────────────────┘")
                    Console.ReadLine()
                    End
                End Try
            End If
        Catch ex As Exception

        End Try

        Console.WriteLine("Trying to access files...")
        Console.ReadLine()
        Try
            CurrentFile = New FileStream(SburbFolderLocation & "Data\Maps\MapIndex.bin", FileMode.Open)
            BinaryWriter = New BinaryWriter(CurrentFile)
            BinaryWriter.Write(TestString)
            BinaryWriter.Close()
            Console.WriteLine("Data Written")
            CurrentFile = New FileStream(SburbFolderLocation & "Data\Maps\MapIndex.bin", FileMode.Open)
            BinaryReader = New BinaryReader(CurrentFile)
            TestString = BinaryReader.Read()
            BinaryWriter.Close()
            Console.WriteLine(TestString)

            Console.WriteLine("Access Successful.")
            Console.ReadLine()
        Catch ex As Exception
            Console.WriteLine(ex)
            Console.WriteLine("Cock.")
            Console.ReadLine()
            End
        End Try


        Console.SetCursorPosition(18, 38)
        Console.WriteLine("Press Enter to begin map generation test")
        Console.ReadLine()
        EventlogSize = -1
        While True
            Try
                TempString = EventLog(EventlogSize + 1)
                EventlogSize += 1
            Catch ex As Exception
                Exit While
            End Try
        End While

        Call MapGen()

    End Sub

    Sub GetDrives(ByRef ValidDrives() As String, ByVal NumOfDrives As Integer)

        NumOfDrives = -1

        ReDim Preserve ValidDrives(NumOfDrives)
        For Ascii As Integer = 65 To 90
            If My.Computer.FileSystem.DirectoryExists(Chr(Ascii) & ":\") Then
                NumOfDrives += 1
                ReDim Preserve ValidDrives(NumOfDrives)
                ValidDrives(NumOfDrives) = Chr(Ascii)
            End If
        Next
    End Sub

    Sub MapGen()

        Dim RoomNum As Integer
        Dim CurrentRoomCenterX As Integer
        Dim CurrentRoomCenterY As Integer
        Dim RoomWidth As Integer
        Dim RoomHeight As Integer
        Dim PosX As Integer
        Dim PosY As Integer
        Dim InitialRoomNum As Integer
        Dim Rand As Integer

        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()

        Dim DestX As Integer
        Dim DestY As Integer

        For count1 As Integer = 0 To 63
            For count2 As Integer = 0 To 39
                SolidMap(count1, count2) = 1
            Next
        Next

        For count1 As Integer = 0 To 63
            For count2 As Integer = 0 To 39
                TerrainMap(count1, count2) = 0
            Next
        Next

        RoomNum = (CInt(((10) * Rnd())) + 4)
        InitialRoomNum = RoomNum

        ReDim Preserve RoomCentres(1, RoomNum - 1)

        While RoomNum <> 0
            CurrentRoomCenterX = CInt((62 - 2) * Rnd())
            CurrentRoomCenterY = CInt((38 - 2) * Rnd())
            While SolidMap(CurrentRoomCenterX, CurrentRoomCenterY) <> 1
                CurrentRoomCenterX = CInt((62 - 2) * Rnd())
                CurrentRoomCenterY = CInt((38 - 2) * Rnd())
            End While
            RoomWidth = CInt((10 - 4) * Rnd()) + 3
            RoomHeight = CInt((10 - 4) * Rnd()) + 3

            PosX = CurrentRoomCenterX - CInt(RoomWidth / 2)
            PosY = CurrentRoomCenterY - CInt(RoomHeight / 2)

            If CurrentRoomCenterX + CInt(RoomWidth / 2) < 62 Then                   'Checks that the room will not be out of bounds by clipping the 'Right' of the map
                If CurrentRoomCenterX - CInt(RoomWidth / 2) > 1 Then                'Checks that the room will not be out of bounds by clipping the 'Left' of the map
                    If CurrentRoomCenterY + CInt(RoomHeight / 2) < 38 Then          'Checks that the room will not be out of bounds by clipping the 'Bottom' of the map  
                        If CurrentRoomCenterY - CInt(RoomHeight / 2) > 1 Then       'Checks that the room will not be out of bounds by clipping the 'Top' of the map


                            For Count4 As Integer = 0 To RoomHeight                                     'While the room has not had all vertical lines drawn
                                For count3 As Integer = 0 To RoomWidth                                  'For 0 to the desired length of the room
                                    SolidMap(PosX, PosY) = 0
                                    PosX += 1
                                Next
                                PosX = CurrentRoomCenterX - CInt(RoomWidth / 2)
                                PosY += 1
                            Next

                            RoomCentres(0, RoomNum - 1) = CurrentRoomCenterX
                            RoomCentres(1, RoomNum - 1) = CurrentRoomCenterY
                            RoomNum -= 1

                        End If
                    End If
                End If
            End If

        End While

        For CountY As Integer = 0 To 39
            For CountX As Integer = 0 To 63
                PosX = CountX
                PosY = CountY
                If TerrainMap(PosX, PosY) = 3 Then
                    Continue For
                ElseIf SolidMap(PosX, PosY) = 0 Then
                    TerrainMap(PosX, PosY) = 1            'Makes the area a normal floor tile (1)
                Else
                    Call TerrainCheck(PosX, PosY, CountX, CountY)
                End If
            Next
        Next

        Rand = CInt((InitialRoomNum - 1) * Rnd())

        PosX = RoomCentres(0, Rand)
        PosY = RoomCentres(1, Rand)

        TerrainMap(PosX, PosY) = 10                     'Makes the area an upwards staircase

        UpStair(0) = PosX
        UpStair(1) = PosY

        While PosX = UpStair(0) And PosY = UpStair(1)
            Rand = CInt((InitialRoomNum - 1) * Rnd())


            PosX = RoomCentres(0, Rand)
            PosY = RoomCentres(1, Rand)
        End While

        TerrainMap(PosX, PosY) = 11                     'Makes the area a downwards staircase

        DownStair(0) = PosX
        DownStair(1) = PosY

        Call CorridorGenerationTest(CurrentRoomCenterX, CurrentRoomCenterY, PosX, PosY, DestX, DestY, InitialRoomNum)

        For CountY As Integer = 0 To 39
            For CountX As Integer = 0 To 63
                PosX = CountX
                PosY = CountY
                If TerrainMap(PosX, PosY) = 3 Then
                    Continue For
                ElseIf TerrainMap(PosX, PosY) = 10 Then
                    Continue For
                ElseIf TerrainMap(PosX, PosY) = 11 Then
                    Continue For
                ElseIf SolidMap(PosX, PosY) = 0 Then
                    TerrainMap(PosX, PosY) = 1           'Makes the area a normal floor tile (1)
                Else
                    Call TerrainCheck(PosX, PosY, CountX, CountY)
                End If
            Next
        Next

        'Call GameLoop(UpStair, DownStair)
        Call GameLoop(UpStair, DownStair)
    End Sub

    Sub TerrainCheck(ByVal PosX As Integer, ByVal PosY As Integer, ByVal CountX As Integer, ByVal CountY As Integer)

        If TerrainMap(PosX, PosY) = 3 Then
            Exit Sub
        End If

        If PosX = 63 Then
            TerrainMap(PosX, PosY) = 0                      'Makes the area black space (0)
            Exit Sub
        ElseIf PosX = 0 Then
            TerrainMap(PosX, PosY) = 0                      'Makes the area black space (0)
            Exit Sub
        ElseIf PosY = 39 Then
            TerrainMap(PosX, PosY) = 0                      'Makes the area black space (0)
            Exit Sub
        ElseIf PosY = 0 Then
            TerrainMap(PosX, PosY) = 0                      'Makes the area black space (0)
            Exit Sub
        End If

        If SolidMap(PosX + 1, PosY) = 0 Then
            TerrainMap(PosX, PosY) = 2                      'Makes the area a normal wall (2)
        End If

        If SolidMap(PosX - 1, PosY) = 0 Then
            TerrainMap(PosX, PosY) = 2                      'Makes the area a normal wall (2)
        End If

        If SolidMap(PosX + 1, PosY + 1) = 0 Then
            TerrainMap(PosX, PosY) = 2                      'Makes the area a normal wall (2)
        End If

        If SolidMap(PosX + 1, PosY - 1) = 0 Then
            TerrainMap(PosX, PosY) = 2                      'Makes the area a normal wall (2)
        End If

        If SolidMap(PosX - 1, PosY + 1) = 0 Then
            TerrainMap(PosX, PosY) = 2                      'Makes the area a normal wall (2)
        End If

        If SolidMap(PosX - 1, PosY - 1) = 0 Then
            TerrainMap(PosX, PosY) = 2                      'Makes the area a normal wall (2)
        End If

        If SolidMap(PosX, PosY + 1) = 0 Then
            TerrainMap(PosX, PosY) = 2                      'Makes the area a normal wall (2)
        End If

        If SolidMap(PosX, PosY - 1) = 0 Then
            TerrainMap(PosX, PosY) = 2                      'Makes the area a normal wall (2)
        End If

    End Sub

    Sub CorridorGeneration(ByVal CurrentRoomCenterX As Integer, ByVal CurrentRoomCenterY As Integer, ByVal PosX As Integer, ByVal PosY As Integer, ByVal DestX As Integer, ByVal DestY As Integer, ByVal RoomNum As Integer)


        For X As Integer = 0 To RoomNum - 1                                 'For each room
            CurrentRoomCenterX = RoomCentres(0, X)                          'Get the X position of the room's center
            CurrentRoomCenterY = RoomCentres(1, X)                          'Get the Y position of the room's center
            Try
                DestX = RoomCentres(0, X + 1)                               'Sets the X destination as the X center of the next room
                DestY = RoomCentres(1, X + 1)                               'Sets the Y destination as the Y center of the next room
            Catch ex As Exception                                           'If the currently selected room is the last in the array
                DestX = RoomCentres(0, 0)                                   'Sets the X destination as the X center of the first room
                DestY = RoomCentres(1, 0)                                   'Sets the Y destination as the Y center of the first room
            End Try

            PosX = CurrentRoomCenterX
            PosY = CurrentRoomCenterY

            While PosX <> DestX
                While PosY <> DestY
                    If DestY > PosY Then
                        PosY += 1
                    ElseIf DestY < PosY Then
                        PosY -= 1
                    End If
                    SolidMap(PosX, PosY) = 0
                    If TerrainMap(PosX, PosY) = 2 Then                      'If the current square is a wall
                        TerrainMap(PosX, PosY) = 3              'Door in terrainmap
                        SolidMap(PosX, PosY) = 2                'Door in solidmap
                    End If
                End While
                If DestX > PosX Then
                    PosX += 1
                ElseIf DestX < PosX Then
                    PosX -= 1
                End If
                SolidMap(PosX, PosY) = 0
                If TerrainMap(PosX, PosY) = 2 Then
                    TerrainMap(PosX, PosY) = 3              'Door in terrainmap
                    SolidMap(PosX, PosY) = 2                'Door in solidmap
                End If
            End While
        Next

    End Sub

    Sub CorridorGenerationTest(ByVal CurrentRoomCenterX As Integer, ByVal CurrentRoomCenterY As Integer, ByVal PosX As Integer, ByVal PosY As Integer, ByVal DestX As Integer, ByVal DestY As Integer, ByVal RoomNum As Integer)


        Dim NumberOfDoors As Integer = 2                                    'Sets the number of doors allowed in the corridor to 2

        For X As Integer = 0 To (RoomNum - 1)                               'For each room
            CurrentRoomCenterX = RoomCentres(0, X)                          'Get the X position of the room's center
            CurrentRoomCenterY = RoomCentres(1, X)                          'Get the Y position of the room's center
            Try
                DestX = RoomCentres(0, X + 1)                               'Sets the X destination as the X center of the next room
                DestY = RoomCentres(1, X + 1)                               'Sets the Y destination as the Y center of the next room
            Catch ex As Exception                                           'If the currently selected room is the last in the array
                DestX = RoomCentres(0, 0)                                   'Sets the X destination as the X center of the first room
                DestY = RoomCentres(1, 0)                                   'Sets the Y destination as the Y center of the first room
            End Try

            PosX = CurrentRoomCenterX
            PosY = CurrentRoomCenterY

            NumberOfDoors = 2

            While PosX <> DestX
                While PosY <> DestY
                    If DestY > PosY Then
                        PosY += 1
                    ElseIf DestY < PosY Then
                        PosY -= 1
                    End If
                    SolidMap(PosX, PosY) = 0
                    If TerrainMap(PosX, PosY) = 2 Then                      'If the current square is a wall
                        If NumberOfDoors <> 0 Then
                            TerrainMap(PosX, PosY) = 3              'Door in terrainmap
                            SolidMap(PosX, PosY) = 2                'Door in solidmap
                            NumberOfDoors -= 1
                        Else
                            Exit While
                        End If
                    End If
                End While
                If DestX > PosX Then
                    PosX += 1
                ElseIf DestX < PosX Then
                    PosX -= 1
                End If
                SolidMap(PosX, PosY) = 0
                If TerrainMap(PosX, PosY) = 2 Then
                    If NumberOfDoors <> 0 Then
                        TerrainMap(PosX, PosY) = 3              'Door in terrainmap
                        SolidMap(PosX, PosY) = 2                'Door in solidmap
                        NumberOfDoors -= 1
                    Else
                        Exit While
                    End If
                End If
            End While
        Next

        PosX = UpStair(0)
        PosY = UpStair(1)

        DestX = DownStair(0)
        DestY = DownStair(1)

        NumberOfDoors = 4

        While PosX <> DestX
            While PosY <> DestY
                If DestY > PosY Then
                    PosY += 1
                ElseIf DestY < PosY Then
                    PosY -= 1
                End If
                SolidMap(PosX, PosY) = 0
                If TerrainMap(PosX, PosY) = 2 Then                      'If the current square is a wall
                    If NumberOfDoors <> 0 Then
                        TerrainMap(PosX, PosY) = 3              'Door in terrainmap
                        SolidMap(PosX, PosY) = 2                'Door in solidmap
                        NumberOfDoors -= 1
                    Else
                        TerrainMap(PosX, PosY) = 1
                        SolidMap(PosX, PosY) = 0
                    End If
                End If
            End While
            If DestX > PosX Then
                PosX += 1
            ElseIf DestX < PosX Then
                PosX -= 1
            End If
            SolidMap(PosX, PosY) = 0
            If TerrainMap(PosX, PosY) = 2 Then
                If NumberOfDoors <> 0 Then
                    TerrainMap(PosX, PosY) = 3              'Door in terrainmap
                    SolidMap(PosX, PosY) = 2                'Door in solidmap
                    NumberOfDoors -= 1
                Else
                    TerrainMap(PosX, PosY) = 1
                    SolidMap(PosX, PosY) = 0
                End If
            End If
        End While

    End Sub

    Sub Rendertest()

        Dim Choice As String

        For Y = 0 To 39
            For X = 0 To 63
                Select Case TerrainMap(X, Y)
                    Case 0
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.Write(" ")
                    Case 1
                        Console.BackgroundColor = ConsoleColor.Gray
                        Console.Write(" ")
                    Case 2
                        Console.BackgroundColor = ConsoleColor.DarkGray
                        Console.Write(" ")
                    Case 3
                        Console.BackgroundColor = ConsoleColor.DarkYellow
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write("%")
                    Case 10
                        Console.BackgroundColor = ConsoleColor.Cyan
                        Console.ForegroundColor = ConsoleColor.White
                        Console.Write("↑")
                    Case 11
                        Console.BackgroundColor = ConsoleColor.DarkRed
                        Console.ForegroundColor = ConsoleColor.White
                        Console.Write("↓")
                End Select
            Next
            Console.SetCursorPosition(0, Y)
        Next

        Choice = Console.ReadLine().ToUpper

        If Choice = "Q" Then
            Exit Sub
        End If

        Console.Clear()

        Call MapGen()
    End Sub

    Sub RenderMap()
        For X As Integer = 0 To 63
            For Y As Integer = 0 To 39
                If VisibilityMap(X, Y) <> 0 Then
                    Call RenderSquare(X, Y)
                End If
            Next
        Next
        Console.ForegroundColor = ConsoleColor.White
        Console.BackgroundColor = ConsoleColor.Black
    End Sub

    Sub RenderSquare(ByVal PosX As Integer, ByVal PosY As Integer)
        Console.SetCursorPosition(PosX, PosY)
        Select Case VisibilityMap(PosX, PosY)
            Case 2
                Select Case TerrainMap(PosX, PosY)
                    Case 0
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.Write(" ")
                    Case 1
                        Console.BackgroundColor = ConsoleColor.Gray
                        Console.Write(" ")
                    Case 2
                        Console.BackgroundColor = ConsoleColor.DarkGray
                        Console.Write(" ")
                    Case 3
                        Console.BackgroundColor = ConsoleColor.DarkYellow
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write("%")
                    Case 4
                        Console.BackgroundColor = ConsoleColor.DarkGray
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write("_")
                    Case 10
                        Console.BackgroundColor = ConsoleColor.Cyan
                        Console.ForegroundColor = ConsoleColor.White
                        Console.Write("↑")
                    Case 11
                        Console.BackgroundColor = ConsoleColor.DarkRed
                        Console.ForegroundColor = ConsoleColor.White
                        Console.Write("↓")
                End Select
            Case 1
                Select Case TerrainMap(PosX, PosY)
                    Case 0
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.Write(" ")
                    Case 1
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.Gray
                        Console.Write("▒")
                    Case 2
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.DarkGray
                        Console.Write("▒")
                    Case 3
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write("▒")
                    Case 4
                        Console.BackgroundColor = ConsoleColor.DarkGray
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write("_")
                    Case 10
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.DarkCyan
                        Console.Write("▒")
                    Case 11
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.DarkRed
                        Console.Write("▒")
                End Select
            Case Else
                Console.BackgroundColor = ConsoleColor.Black
                Console.Write(" ")
        End Select
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White
    End Sub

    Sub GameLoop(ByVal UpStairs() As Integer, ByVal DownStairs() As Integer)
        Dim PosX As Integer
        Dim PosY As Integer
        Dim CursorX As Integer
        Dim CursorY As Integer
        Dim Key As Integer
        Dim ExitLoop As Boolean
        Dim OldPosX As Integer
        Dim OldPosY As Integer
        Dim InteractChoiceMade As Boolean = False

        PosX = UpStair(0)
        PosY = UpStair(1)

        For X As Integer = 0 To 63
            For Y As Integer = 0 To 39
                VisibilityMap(X, Y) = 0
            Next
        Next


        Call LineOfSight(PosX, PosY)
        Call RenderMap()

        Console.SetCursorPosition(PosX, PosY)
        Console.BackgroundColor = ConsoleColor.Cyan
        Console.ForegroundColor = ConsoleColor.White
        Console.Write("☺")
        Entitymap(PosX, PosY) = 1                                            'Marks the player on the entity map.

        ExitLoop = False
        While ExitLoop = False
            OldPosX = PosX
            OldPosY = PosY
            Key = Console.ReadKey(True).Key
            Select Case Key
                Case 38                                                     'Up arrow
                    Try
                        If SolidMap(PosX, PosY - 1) <> 1 Then
                            PosY = PosY - 1
                            Console.SetCursorPosition(PosX, PosY - 1)
                        End If
                    Catch ex As Exception
                        Continue While
                    End Try

                Case 40                                                     'Down arrow
                    Try
                        If SolidMap(PosX, PosY + 1) <> 1 Then
                            PosY = PosY + 1
                            Console.SetCursorPosition(PosX, PosY + 1)
                        End If
                    Catch ex As Exception
                        Continue While
                    End Try
                Case 37                                                     'Left arrow
                    Try
                        If SolidMap(PosX - 1, PosY) <> 1 Then
                            PosX = PosX - 1
                            Console.SetCursorPosition(PosX - 1, PosY)
                        End If
                    Catch ex As Exception
                        Continue While
                    End Try
                Case 39                                                     'Right arrow
                    Try
                        If SolidMap(PosX + 1, PosY) <> 1 Then
                            PosX = PosX + 1
                            Console.SetCursorPosition(PosX + 1, PosY)
                        End If
                    Catch ex As Exception
                        Continue While
                    End Try
                Case 75                                                    'K key
                    InteractChoiceMade = False
                    CursorX = PosX
                    CursorY = PosY
                    While InteractChoiceMade = False
                        Console.SetCursorPosition(PosX, PosY)
                        Select Case TerrainMap(PosX, PosY)
                            Case 0
                                Console.BackgroundColor = ConsoleColor.Black
                            Case 1
                                Console.BackgroundColor = ConsoleColor.Gray
                            Case 2
                                Console.BackgroundColor = ConsoleColor.DarkGray
                            Case 3
                                Console.BackgroundColor = ConsoleColor.DarkYellow
                            Case 10
                                Console.BackgroundColor = ConsoleColor.Cyan
                            Case 11
                                Console.BackgroundColor = ConsoleColor.DarkRed
                        End Select
                        Console.Write("☺")
                        Console.SetCursorPosition(CursorX, CursorY)
                        Select Case TerrainMap(CursorX, CursorY)
                            Case 0
                                Console.BackgroundColor = ConsoleColor.Black
                            Case 1
                                Console.BackgroundColor = ConsoleColor.Gray
                            Case 2
                                Console.BackgroundColor = ConsoleColor.DarkGray
                            Case 3
                                Console.BackgroundColor = ConsoleColor.DarkYellow
                            Case 10
                                Console.BackgroundColor = ConsoleColor.Cyan
                            Case 11
                                Console.BackgroundColor = ConsoleColor.DarkRed
                        End Select
                        Console.Write("X")
                        Key = Console.ReadKey(True).Key
                        Select Case Key
                            Case 37                                         'Left arrow
                                If SolidMap(CursorX - 1, CursorY) <> 1 Then
                                    If VisibilityMap(CursorX - 1, CursorY) <> 0 Then
                                        Call RenderSquare(CursorX, CursorY)
                                        CursorX -= 1
                                    End If
                                End If
                            Case 38                                         'Up arrow
                                If SolidMap(CursorX, CursorY - 1) <> 1 Then
                                    If VisibilityMap(CursorX, CursorY - 1) <> 0 Then
                                        Call RenderSquare(CursorX, CursorY)
                                        CursorY -= 1
                                    End If
                                End If
                            Case 39                                         'Right arrow
                                If SolidMap(CursorX + 1, CursorY) <> 1 Then
                                    If VisibilityMap(CursorX + 1, CursorY) <> 0 Then
                                        Call RenderSquare(CursorX, CursorY)
                                        CursorX += 1
                                    End If
                                End If
                            Case 40                                         'Down arrow
                                If SolidMap(CursorX, CursorY + 1) <> 1 Then
                                    If VisibilityMap(CursorX, CursorY + 1) <> 0 Then
                                        Call RenderSquare(CursorX, CursorY)
                                        CursorY += 1
                                    End If
                                End If
                            Case 27                                         'Esc Key
                                Call RenderSquare(CursorX, CursorY)
                                Console.SetCursorPosition(PosX, PosY)
                                InteractChoiceMade = True
                            Case 13                                         'Enter Key
                                Call InteractSquare(CursorX, CursorY)
                                Call RenderSquare(CursorX, CursorY)
                                Console.SetCursorPosition(PosX, PosY)
                                InteractChoiceMade = True
                        End Select
                    End While
                Case 27
                    ExitLoop = True
            End Select

            Entitymap(PosX, PosY) = 1                                       'Marks the player on the entity map.

            Try
                Call LineOfSight(PosX, PosY)
                Call RenderMap()
                Console.SetCursorPosition(OldPosX, OldPosY)
                Call RenderSquare(OldPosX, OldPosY)
                Console.SetCursorPosition(PosX, PosY)
                Select Case TerrainMap(PosX, PosY)
                    Case 0
                        Console.BackgroundColor = ConsoleColor.Black
                    Case 1
                        Console.BackgroundColor = ConsoleColor.Gray
                    Case 2
                        Console.BackgroundColor = ConsoleColor.DarkGray
                    Case 3
                        Console.BackgroundColor = ConsoleColor.DarkYellow
                    Case 10
                        Console.BackgroundColor = ConsoleColor.Cyan
                    Case 11
                        Console.BackgroundColor = ConsoleColor.DarkRed
                End Select
                Console.ForegroundColor = ConsoleColor.White
                Console.Write("☺")
            Catch ex As Exception
                Continue While
            End Try
            If PosX = DownStair(0) Then
                If PosY = DownStair(1) Then
                    Call MapGen()
                End If
            End If
            Console.SetCursorPosition(PosX, PosY)
        End While
    End Sub

    Sub LineOfSight(ByVal PosX As Integer, ByVal PosY As Integer)
        Dim X As Double
        Dim Y As Double
        Dim RayCount As Double

        For TempX As Integer = 0 To 63
            For TempY As Integer = 0 To 39
                If VisibilityMap(TempX, TempY) = 2 Then
                    VisibilityMap(TempX, TempY) = 1
                Else
                    VisibilityMap(TempX, TempY) = 0
                End If
            Next
        Next

        For RayCount = 0 To 360 Step 0.25
            X = Math.Cos(RayCount * 0.0174532925)
            Y = Math.Sin(RayCount * 0.0174532925)
            Call FOVCheck(X, Y, PosX, PosY)
        Next
    End Sub

    Sub FOVCheck(ByVal X As Double, ByVal Y As Double, ByVal PlayerX As Integer, ByVal PlayerY As Integer)

        Dim I As Integer
        Dim RayPosX As Double
        Dim RayPosY As Double

        RayPosX = (PlayerX)
        RayPosY = (PlayerY)

        For I = 0 To 100 Step 1              'where 100 is the maximum view range
            VisibilityMap(CInt(RayPosX), CInt(RayPosY)) = 2
            If SolidMap(CInt(RayPosX), CInt(RayPosY)) = 1 Then
                Return
            ElseIf SolidMap(CInt(RayPosX), CInt(RayPosY)) = 2 Then
                Return
            End If

            RayPosX += X
            RayPosY += Y
        Next
    End Sub

    Sub TestLineOfSight(ByVal PosX As Integer, ByVal PosY As Integer)
        Dim CurrentX As Integer
        Dim CurrentY As Integer
        Dim Gradient As Integer
        Dim IsSquareVisible As Boolean

        For TempX As Integer = 0 To 63
            For TempY As Integer = 0 To 39
                If VisibilityMap(TempX, TempY) = 2 Then
                    VisibilityMap(TempX, TempY) = 1
                Else
                    VisibilityMap(TempX, TempY) = 0
                End If
            Next
        Next

        For SquareToCheckX As Integer = (PosX - 5) To (PosX + 5)
            For SquareToCheckY As Integer = (PosY - 5) To (PosY + 5)
                Try
                    IsSquareVisible = True
                    CurrentX = SquareToCheckX
                    CurrentY = SquareToCheckY
                    Gradient = (PosX - CurrentX) / (PosY - CurrentY)            'Works out the gradient of the 'line'
                    While (CurrentX <> PosX And CurrentY <> PosY)
                        If CurrentX < PosX Then
                            CurrentX += 1
                        ElseIf CurrentX > PosX Then
                            CurrentX -= 1
                        End If
                        CurrentY = CurrentY + Gradient
                        If SolidMap(CurrentX, CInt(CurrentY)) = 1 Then
                            IsSquareVisible = False
                            Exit While
                        End If
                    End While
                    If IsSquareVisible = False Then
                        VisibilityMap(SquareToCheckX, SquareToCheckY) = 0
                    ElseIf IsSquareVisible = True Then
                        VisibilityMap(SquareToCheckX, SquareToCheckY) = 2
                    End If
                Catch ex As Exception
                End Try
            Next
        Next
    End Sub

    Sub InteractSquare(ByVal CursorX As Integer, ByVal CursorY As Integer)
        Dim OutputString As String
        Dim Item As Boolean = False                         'Is there an item on the square?
        Dim Entity As Boolean = False                       'Is there an entity in the square?
        Dim Description As String                           'Description of the square
        If ItemMap(CursorX, CursorY) <> "" Then             'If there is an item in the square, set Item to true.
            Item = True
        End If
        If Entitymap(CursorX, CursorY) <> 0 Then            'If there is an entity in the square, set Entity to true.
            Entity = True
        End If

        If Item = False Then
            If Entity = False Then
                Select Case TerrainMap(CursorX, CursorY)
                    Case 0
                        Description = "a tile you should not be able to see. Kindly refrain from doing what it is that you're doing."
                    Case 1
                        Description = "a stone floor, worked smooth by centuries of use by the dungeons various denizens."
                    Case 2
                        Description = "a roughly hewn stone wall."
                    Case 3
                        Description = "an imposing wooden door, set into the stone around it."
                    Case 4
                        Description = "an open wooden door."
                    Case 10
                        Description = "a crudely carved staircase leading upwards."
                    Case 11
                        Description = "a crudely carved staircase leading downwards."
                End Select
                Console.SetCursorPosition(2, 41)
                OutputString = ("Here lies " & Description)
                Console.WriteLine(OutputString)
                ReDim Preserve EventLog(EventlogSize + 1)
                EventlogSize += 1
                EventLog(EventlogSize) = OutputString
                Console.ReadLine()
                Console.SetCursorPosition(2, 41)
                Console.ForegroundColor = ConsoleColor.White
                Console.BackgroundColor = ConsoleColor.Black
                For I As Integer = 0 To OutputString.Length
                    Console.Write(" ")
                Next
                Return
            End If
        End If
    End Sub
End Module
