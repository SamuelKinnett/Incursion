Imports System.IO
Imports System.Threading
Module Module1

    'The greaterest VB roguelike that ever did a making of for great good!
    'Written by Samuel Kinnett, 2013

    Dim IncursionFolderLocation As String   'The location of the Incursion folder
    Dim NewMapID As Integer                 'The newest available MapID
    Dim CurrentMapID As Integer             'The MapID currently being used
    Dim CurrentFloor As Integer             'The current level of the dungeon that the player is on
    Dim MaxEnemyLevel As Integer            'The highest level of enemies on the current floor
    Dim CurrentDungeonName As String        'The name of the dungeon the player is currently in

    Dim BinaryWriter As BinaryWriter        'Used to write to binary files
    Dim BinaryReader As BinaryReader        'Used to read from binary files
    Dim TextReader As StreamReader          'Used to read in text files
    Dim TextWriter As StreamWriter          'Used to write to text files
    Dim CurrentFile As FileStream           'The file being accessed

    Dim SolidMap(63, 39) As Integer         'this map consists of 1's and 0's. A 1 indicates solid space e.g. a wall, and a 0 indicates empty space that the player can walk through. A door is indicated by 2.
    Dim TerrainMap(63, 39) As Integer       'this map consists of a variety of numbers indicating the material of the 'block' in that space, e.g. stone or wood. A door is indicated by 3.
    Dim ItemMap(63, 39) As String           'this map contains the location of items in the world. Multiple items in one square are divided by a semicolon, e.g. "WOODEN SWORD:WOODEN SHIELD"
    Dim VisibilityMap(63, 39) As Integer    'this array contains a map showing the currently visible, and previously explored, areas of the map.
    Dim SaveVisibilityMap(63, 39) As Integer 'this array is used to save previously visible tiles for saving.
    Dim Entitymap(63, 39) As Integer        'this array contains the locations of all of the entities in the level.
    Dim FloodMap(63, 39) As Integer         'this array is used to flood the map, to find out which squares are reachable and which are not.
    Dim RoomMap(63, 39) As Integer          'this array is used to determine which areas of the map are rooms and which are corridors. 0 is unreachable space, 1 is a corridor and 2 is a room.
    Dim WorldMap(,) As Integer              'this array stores the overworld.
    Dim SaltWater(,) As Integer             'this array stores the fresh and salt water of the map
    Dim WorldWidth As Integer
    Dim WorldHeight As Integer
    Dim OverworldX As Integer
    Dim OverworldY As Integer

    Dim EventLog() As String                'stores all 'EventBox' output for future perusal
    Dim EventlogSize As Integer             'The size of the Event log

    Dim RoomCentres(,) As Integer           'stores the centres of each room for the purposes of linking them via corridors.
    Dim UpStair(1) As Integer               'the X and Y coordinates of the Upwards staircase
    Dim DownStair(1) As Integer             'the X and Y coordinates of the downwards staircase

    Dim WeaponTable(6, 0) As String         'Name, Required Strength, Attack Bonus, One or Two handed (1 = 1 handed, 2 = 2 handed), Weight, Value, Description.
    Dim WeaponTableSize As Integer
    Dim ArmourTable(6, 0) As String         'Name, Required Strength, Defence Bonus, Equip Location (0 = Head, 1 = Torso, 2 = Legs, 3 = Arms, 4 = Shield), Weight, Value, Description.
    Dim ArmourTableSize As Integer
    Dim ConsumableTable(6, 0) As String     'Name, Health Bonus, Mana Bonus, Hunger Bonus, Weight, Value, Description.
    Dim ConsumableTableSize As Integer
    Dim SpellTable(6, 0) As String          'Name, Required Wisdom, Mana Cost, Magic Type (0 = Projectile, 1 = Offensive Buff, 2 = Defensive Buff, 3 = Healing, 4 = Summoning), Value (e.g. healing ammount, i.d. of npc to summon...), Cooldown Period, Description.
    Dim SpellTableSize As Integer

    Dim TerrainLookupTable(2, 0) As String 'foreground colour/ background colour/ text to write (the picture, effectively); 

    '0  -   Black Space
    '1  -   Stone Floor
    '2  -   Stone Wall
    '3  -   Closed Door
    '4  -   Open Door
    '5  -   Upwards Staircase
    '6  -   Downwards Staircase
    '7  -   Salt Water
    '8  -   Sand Floor
    '9  -   Grass Floor
    '10 -   Fresh Water
    '11 -   Mountain
    '12 -   Forest
    '13 -   Water/Sand
    '14 -   River Vertical │
    '15 -   River Horizontal ─
    '16 -   River ┤
    '17 -   River ├
    '18 -   River ┴
    '19 -   River ┬
    '20 -   River ┼
    '21 -   River ┐
    '22 -   River └
    '23 -   River ┌
    '24 -   River ┘

    Dim ConsoleOutput As New CGFX

    Dim Player As New UserControlledCharacter

    Dim Enemy() As Entity
    Dim EnemyNumber As Integer

    Dim Bullet As New Projectile

#Region "Main Menus"

    Sub TestingMenu()
        Dim Choice As Char
        Dim SubChoice As Integer
        Dim NumberOfFloors As Integer
        Dim DateString As String
        Dim TempCount As Integer
        Dim RejectedWorlds As Integer = 0

        DateString = System.DateTime.UtcNow


        Console.Clear()
        Console.WriteLine("DEVELOPER CONSOLE                                      " & DateString)
        Console.SetCursorPosition(0, 5)
        Console.WriteLine("A - Start Game")
        Console.WriteLine("B - Generate Dungeon")
        Console.WriteLine("C - MapGen Test")
        Console.WriteLine("D - FloodFill Test")
        Console.WriteLine("E - New Item")
        Console.WriteLine("F - View Items")
        Console.WriteLine("G - Overworld Test")
        Console.WriteLine("H - World Gen Test")
        Console.WriteLine()
        Console.Write("::: ")
        Choice = Console.ReadLine.ToUpper

        Select Case Choice
            Case "A"
                Console.Clear()
                Call DeleteAllPlayerData()
                Player.Name = "Samuel Thunderfist"
                Player.Role = "Warrior"
                Player.MaxHealth = 100
                Player.Health = 100
                Player.MaxMana = 30
                Player.Mana = 30
                Player.Strength = 5
                Player.Toughness = 5
                Player.RangedWeapons = 4
                Player.MeleeWeapons = 6
                Player.Wisdom = 3
                Player.Medicine = 5
                Player.Bartering = 4
                Player.Gold = 200
                Player.EquippedWeapon = "N"   'Fists
                Player.Equipment(0) = 1     'Cervelliere
                Player.Equipment(1) = 10    'Iron Hauberk
                Player.Equipment(2) = 18    'Iron Chausses
                Player.Equipment(3) = 25    'Iron Vambraces
                Player.Equipment(4) = 34    'Iron Buckler
                Player.InventorySize = 1
                ReDim Player.Inventory(2, Player.InventorySize)
                Player.Inventory(0, 0) = 2
                Player.Inventory(1, 0) = 1
                Player.Inventory(2, 0) = 4
                Player.Inventory(0, 1) = 0
                Player.Inventory(1, 1) = 0
                Player.Inventory(2, 1) = 1
                Player.Experience(0) = 0
                Player.Experience(1) = 100
                Player.Hunger = 20

                Call GenerateWorld(0)
            Case "B"
                Console.Clear()
                Console.WriteLine("Number of Floors: ")
                NumberOfFloors = Console.ReadLine()
                WorldWidth = 63
                WorldHeight = 39
                Call GenerateWorld(0)
                Call DungeonGen(NumberOfFloors, 30)
                Call TestingMenu()
            Case "C"
                Console.Clear()
                Call MapGenTest()
                Call ConsoleOutput.RenderMap()
                Console.ReadLine()
                Call TestingMenu()
            Case "D"
                Console.Clear()
                Call MapGen()
                Call FloodFill(UpStair(0), UpStair(1))
                Console.ReadLine()
                Call TestingMenu()
            Case "E"
                Console.Clear()
                Console.WriteLine("Weapon (0), Armour (1), Consumable (2) or Spell (3)?")
                SubChoice = Console.ReadLine()
                Select Case SubChoice
                    Case 0
                        Call AddWeapon()
                    Case 1
                        Call AddArmour()
                    Case 2
                        Call AddConsumable()
                    Case 3
                        Call AddSpell()
                End Select
                Call TestingMenu()
            Case "F"
                Call LoadItems()
                Console.WriteLine("Weapon (0), Armour (1), Consumable (2) or Spell (3)?")
                SubChoice = Console.ReadLine()
                Select Case SubChoice
                    Case 0
                        Call ViewItem(0)
                    Case 1
                        Call ViewItem(1)
                    Case 2
                        Call ViewItem(2)
                    Case 3
                        Call ViewItem(3)
                End Select
                Call TestingMenu()
            Case "G"
                Call GenerateWorld(0)
                OverworldX = 40
                OverworldY = 40
                Call Overworld()
            Case "H"
                WorldWidth = 63
                WorldHeight = 39
                Console.Clear()
                Call GenerateWorld(0)
            Case Else
                Call TestingMenu()
        End Select
    End Sub

    Sub Main()

        Console.WindowWidth = 90
        Console.WindowHeight = 60
        Console.CursorVisible = False
        Console.Title = "Incursion Alpha 0.1"

        My.Computer.Audio.Play(My.Resources.Incursion_Main_Theme_Test1, AudioPlayMode.Background)

        Call RenderLoadMenu()
        Console.ReadLine()

        'My.Computer.Audio.Play(My.Resources._04_Sburban_Jungle__Brief_Mix_, AudioPlayMode.Background)
        Call LoadData()
    End Sub

    Sub RenderLoadMenu()
        Console.SetCursorPosition(0, 0)
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Gray
        Console.Clear()
        Console.SetCursorPosition(59, 20)
        Console.Write("██")
        Console.SetCursorPosition(10, 21)
        Console.Write("████                                             ██")
        Console.SetCursorPosition(11, 22)
        Console.Write("██")
        Console.SetCursorPosition(11, 23)
        Console.Write("██    █████    ████   ██  ██  ██  ██   █████  ████     ████   █████")
        Console.SetCursorPosition(11, 24)
        Console.Write("██    ██  ██  ██  ██  ██  ██  ██ ███  ██        ██    ██  ██  ██  ██")
        Console.SetCursorPosition(11, 25)
        Console.Write("██    ██  ██  ██      ██  ██  ███     ██        ██    ██  ██  ██  ██")
        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.BackgroundColor = ConsoleColor.Gray
        Console.SetCursorPosition(11, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(11, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(17, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(17, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(21, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(21, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(25, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(25, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(33, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(33, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(37, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(37, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(41, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(41, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(50, 26)
        Console.Write("▒▒▒▒")
        Console.SetCursorPosition(53, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(59, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(59, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(65, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(65, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(69, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(69, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(73, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(73, 27)
        Console.Write("▒▒")
        Console.SetCursorPosition(77, 26)
        Console.Write("▒▒")
        Console.SetCursorPosition(77, 27)
        Console.Write("▒▒")
        Console.ForegroundColor = ConsoleColor.DarkGray
        Console.BackgroundColor = ConsoleColor.Black
        Console.SetCursorPosition(11, 28)
        Console.Write("██    ██  ██  ██  ██  ██  ██  ██          ██    ██    ██  ██  ██  ██")
        Console.ForegroundColor = ConsoleColor.Black
        Console.BackgroundColor = ConsoleColor.DarkGray
        Console.SetCursorPosition(10, 29)
        Console.Write("▒▒▒▒")
        Console.SetCursorPosition(17, 29)
        Console.Write("▒▒")
        Console.SetCursorPosition(21, 29)
        Console.Write("▒▒")
        Console.SetCursorPosition(26, 29)
        Console.Write("▒▒▒▒")
        Console.SetCursorPosition(34, 29)
        Console.Write("▒▒▒▒")
        Console.SetCursorPosition(41, 29)
        Console.Write("▒▒")
        Console.SetCursorPosition(49, 29)
        Console.Write("▒▒▒▒▒")
        Console.SetCursorPosition(57, 29)
        Console.Write("▒▒▒▒▒▒")
        Console.SetCursorPosition(66, 29)
        Console.Write("▒▒▒▒")
        Console.SetCursorPosition(73, 29)
        Console.Write("▒▒")
        Console.SetCursorPosition(77, 29)
        Console.Write("▒▒")

        Console.BackgroundColor = ConsoleColor.DarkGray
        Console.ForegroundColor = ConsoleColor.DarkGray

        For Y = 0 To 59
            Console.SetCursorPosition(0, Y)
            Console.Write("█")
            Console.SetCursorPosition(89, Y)
            Console.Write("█")
        Next

        For X = 0 To 89
            Console.SetCursorPosition(X, 0)
            Console.Write("█")
            Console.SetCursorPosition(X, 59)
            Console.Write("█")
        Next

        Console.ForegroundColor = ConsoleColor.Black

        For Y = 1 To 58
            Console.SetCursorPosition(1, Y)
            Console.Write("▒")
            Console.SetCursorPosition(88, Y)
            Console.Write("▒")
        Next

        For X = 1 To 88
            Console.SetCursorPosition(X, 1)
            Console.Write("▒")
            Console.SetCursorPosition(X, 58)
            Console.Write("▒")
        Next

        Console.SetCursorPosition(0, 0)

        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White

        Console.SetCursorPosition(33, 34)
        Console.Write("[Press Enter to Begin]")

        'Console.WriteLine()
        'Console.WriteLine("  ┌──────────────────────────────────────────────────────────────────────────┐")
        'Console.WriteLine("  │                                                                          │")
        'Console.WriteLine("  └──────────────────────────────────────────────────────────────────────────┘")

    End Sub

    Sub MainMenu()
        Dim Key As Integer
        Dim Choice As Integer = 1
        ConsoleOutput.RenderMainMenu()
        Console.SetCursorPosition(0, 0)

        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White

        If Choice = 1 Then
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Gray
        End If
        Console.SetCursorPosition(3, 15)
        Console.Write(">>→ NEW GAME")
        Console.ForegroundColor = ConsoleColor.White
        Console.BackgroundColor = ConsoleColor.Black
        If Choice = 2 Then
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Gray
        End If
        Console.SetCursorPosition(3, 20)
        Console.Write(">>→ LOAD GAME")
        Console.ForegroundColor = ConsoleColor.White
        Console.BackgroundColor = ConsoleColor.Black
        If Choice = 3 Then
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Gray
        End If
        Console.SetCursorPosition(3, 25)
        Console.Write(">>→ TESTING MENU")

        While True
            Key = Console.ReadKey(True).Key
            Select Case Key
                Case 38     'Up arrow
                    If Choice > 1 Then
                        Choice -= 1
                    End If
                Case 40     'Down Arrow
                    If Choice < 3 Then
                        Choice += 1
                    End If
                Case 13     'Enter Key
                    If Choice = 1 Then
                        Call NewGame()
                    ElseIf Choice = 2 Then
                        'Load game
                    ElseIf Choice = 3 Then
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.White
                        Call TestingMenu()
                    End If
            End Select

            Console.SetCursorPosition(0, 0)

            Console.BackgroundColor = ConsoleColor.Black
            Console.ForegroundColor = ConsoleColor.White

            If Choice = 1 Then
                Console.ForegroundColor = ConsoleColor.White
                Console.BackgroundColor = ConsoleColor.Gray
            End If
            Console.SetCursorPosition(3, 15)
            Console.Write(">>→ NEW GAME")
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
            If Choice = 2 Then
                Console.ForegroundColor = ConsoleColor.White
                Console.BackgroundColor = ConsoleColor.Gray
            End If
            Console.SetCursorPosition(3, 20)
            Console.Write(">>→ LOAD GAME")
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
            If Choice = 3 Then
                Console.ForegroundColor = ConsoleColor.White
                Console.BackgroundColor = ConsoleColor.Gray
            End If
            Console.SetCursorPosition(3, 25)
            Console.Write(">>→ TESTING MENU")

        End While
    End Sub

#End Region

#Region "File Management"

    Sub LoadData()

        Dim ValidDrives() As String                 'All the hard drives on the computer
        Dim NumOfDrives As Integer                  'The number of drives on the computer
        Dim DirectoryFound As Boolean = False       'Has the Incursion directory been found?
        Dim ChosenDrive As String                   'The drive to save Incursion to
        Dim Key As Integer                          'Variable for keyboard input
        Dim CurrentlySelectedDriveNumber As Integer
        Dim NumberOfMaps As Integer                 'The number of maps saved in the maps folder

        Randomize()


        Call GetDrives(ValidDrives, NumOfDrives)

        'Code will go here that checks if the necessary files are in place, and creates them if they aren't. It then loads all the random names into the arrays.

        Console.SetCursorPosition(30, 36)
        Console.Write("Checking File Directories")
        Try
            For DriveCount As Integer = 0 To NumOfDrives
                If My.Computer.FileSystem.DirectoryExists(ValidDrives(DriveCount) & ":\Incursion") Then
                    DirectoryFound = True
                    IncursionFolderLocation = ValidDrives(DriveCount) & ":\Incursion\"
                    Exit For
                End If
            Next
        Catch ex As Exception
            'Error catching stuff can go here
        End Try
        If DirectoryFound = False Then
            Console.SetCursorPosition(20, 20)
            Console.Write("┌────────────────────────────────────────┐")
            Console.SetCursorPosition(20, 21)
            Console.Write("│ ERROR: MAIN DIRECTORY NOT FOUND!       │")
            Console.SetCursorPosition(20, 22)
            Console.Write("│ SELECT DRIVE TO INSTALL DIRECTORY:     │")
            Console.SetCursorPosition(20, 23)
            Console.Write("│                                        │")
            For X As Integer = 24 To (24 + NumOfDrives)
                Console.SetCursorPosition(20, X)
                Console.Write("│ " & ValidDrives(X - 24) & ":\                                    ")
                Console.SetCursorPosition(61, X)
                Console.WriteLine("│")
            Next
            Console.SetCursorPosition(20, 25 + NumOfDrives)
            Console.Write("└────────────────────────────────────────┘")
            'Console.SetCursorPosition(20, (25 + NumOfDrives) + 1)
            'Console.Write("│ ENTER CUSTOM FILEPATH")
            'Console.SetCursorPosition(61, (25 + NumOfDrives) + 1)
            'Console.WriteLine("│")
            'Console.SetCursorPosition(20, (25 + NumOfDrives) + 2)
            'Console.Write("└────────────────────────────────────────┘")

            CurrentlySelectedDriveNumber = 0
            ChosenDrive = Nothing
            While ChosenDrive = Nothing
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
                        If CurrentlySelectedDriveNumber > 0 Then
                            CurrentlySelectedDriveNumber -= 1
                        End If
                    Case 40
                        If CurrentlySelectedDriveNumber < NumOfDrives Then
                            CurrentlySelectedDriveNumber += 1
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
                My.Computer.FileSystem.CreateDirectory(ChosenDrive & "Incursion")
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

        Console.SetCursorPosition(0, 36)
        Console.WriteLine("                   Checking necessary folders are in place                     ")

        Try
            If My.Computer.FileSystem.DirectoryExists(IncursionFolderLocation & "Data") Then

                If My.Computer.FileSystem.DirectoryExists(IncursionFolderLocation & "Data\Maps") Then

                    If My.Computer.FileSystem.DirectoryExists(IncursionFolderLocation & "Data\Player") Then

                        If My.Computer.FileSystem.DirectoryExists(IncursionFolderLocation & "Data\Items") Then

                        Else
                            Try
                                Console.SetCursorPosition(20, 20)
                                Console.Write("┌────────────────────────────────────────┐")
                                Console.SetCursorPosition(20, 21)
                                Console.Write("│         CREATING ITEMS FOLDER          │")
                                Console.SetCursorPosition(20, 22)
                                Console.Write("└────────────────────────────────────────┘")
                                System.Threading.Thread.Sleep(500)
                                My.Computer.FileSystem.CreateDirectory(IncursionFolderLocation & "Data\Items")
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
                            My.Computer.FileSystem.CreateDirectory(IncursionFolderLocation & "Data\Player")
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
                        My.Computer.FileSystem.CreateDirectory(IncursionFolderLocation & "Data\Maps")
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
                    My.Computer.FileSystem.CreateDirectory(IncursionFolderLocation & "Data")
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


        Console.SetCursorPosition(5, 36)
        Console.WriteLine("                    Checking necessary files are in place                      ")


        Try
            If My.Computer.FileSystem.FileExists(IncursionFolderLocation & "Data\Maps\MapIndex.bin") Then

                If My.Computer.FileSystem.FileExists(IncursionFolderLocation & "Data\Player\Config.txt") Then

                    If My.Computer.FileSystem.FileExists(IncursionFolderLocation & "Data\Player\PlayerData.bin") Then


                    Else
                        Try
                            Console.SetCursorPosition(20, 20)
                            Console.Write("┌────────────────────────────────────────┐")
                            Console.SetCursorPosition(20, 21)
                            Console.Write("│        CREATING PLAYER DATA FILE       │")
                            Console.SetCursorPosition(20, 22)
                            Console.Write("└────────────────────────────────────────┘")
                            System.Threading.Thread.Sleep(500)
                            File.Create(IncursionFolderLocation & "Data\Player\PlayerData.bin").Dispose()
                            Call RenderLoadMenu()
                            Call LoadData()
                        Catch ex As Exception
                            My.Computer.Audio.Stop()
                            Console.SetCursorPosition(18, 20)
                            Console.Write("┌───────────────────────────────────────────┐")
                            Console.SetCursorPosition(18, 21)
                            Console.Write("│ ERROR! UNABLE TO CREATE PLAYER DATA FILE! │")
                            Console.SetCursorPosition(18, 22)
                            Console.Write("└───────────────────────────────────────────┘")
                            Console.ReadLine()
                            End
                        End Try
                    End If
                Else
                    Try
                        Console.SetCursorPosition(20, 20)
                        Console.Write("┌────────────────────────────────────────┐")
                        Console.SetCursorPosition(20, 21)
                        Console.Write("│          CREATING CONFIG FILE          │")
                        Console.SetCursorPosition(20, 22)
                        Console.Write("└────────────────────────────────────────┘")
                        System.Threading.Thread.Sleep(500)
                        File.Create(IncursionFolderLocation & "Data\Player\Config.txt").Dispose()
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
                    File.Create(IncursionFolderLocation & "Data\Maps\MapIndex.bin").Dispose()
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

        NumberOfMaps = -1

        Console.SetCursorPosition(5, 36)
        Console.WriteLine("                       Checking for exisiting map files                        ")
        Try
            While True
                NumberOfMaps += 1
                CurrentFile = New FileStream(IncursionFolderLocation & "\Data\Maps\MAP" & NumberOfMaps, FileMode.Open)
                BinaryReader = New BinaryReader(CurrentFile)
                BinaryReader.Close()
            End While
        Catch ex As Exception
            NewMapID = NumberOfMaps
        End Try

        'Console.SetCursorPosition(18, 38)
        'Console.WriteLine("Press Enter to begin map generation test")
        'Console.ReadLine()
        EventlogSize = -1

        Console.SetCursorPosition(5, 36)
        Console.WriteLine("                                 Loading Items                                 ")
        Call LoadItems()

        Console.SetCursorPosition(5, 36)
        Console.WriteLine("                                Loading Terrain                                ")
        Call LoadTerrain()

        Console.Clear()

        Call MainMenu()

    End Sub

    Sub AddWeapon()
        Dim Input As String
        TextWriter = New StreamWriter(IncursionFolderLocation & "Data\Items\Items.txt", True)

        Console.CursorVisible = True

        Console.Clear()
        Input = "0"
        Console.Write("Name: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Required Strength: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Attack Bonus: ")
        Input += ";" & Console.ReadLine()
        Console.Write("1 or 2 Handed: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Weight: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Value: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Description: ")
        Input += ";" & Console.ReadLine()

        TextWriter.WriteLine(Input)
        TextWriter.Close()

        Console.CursorVisible = False

        Console.Clear()
        Return

    End Sub

    Sub AddArmour()
        Dim Input As String
        TextWriter = New StreamWriter(IncursionFolderLocation & "Data\Items\Items.txt", True)

        Console.CursorVisible = True

        Console.Clear()
        Input = "1"
        Console.Write("Name: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Required Strength: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Defense Bonus: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Equip Location (0 = Head, 1 = Torso, 2 = Legs): ")
        Input += ";" & Console.ReadLine()
        Console.Write("Weight: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Value: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Description: ")
        Input += ";" & Console.ReadLine()

        TextWriter.WriteLine(Input)
        TextWriter.Close()

        Console.CursorVisible = False

        Console.Clear()
        Return

    End Sub

    Sub AddConsumable()
        Dim Input As String
        TextWriter = New StreamWriter(IncursionFolderLocation & "Data\Items\Items.txt", True)

        Console.CursorVisible = True

        Console.Clear()
        Input = "2"
        Console.Write("Name: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Health Bonus: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Mana Bonus: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Hunger Bonus: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Weight: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Value: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Description: ")
        Input += ";" & Console.ReadLine()

        TextWriter.WriteLine(Input)
        TextWriter.Close()

        Console.CursorVisible = False

        Console.Clear()
        Return

    End Sub

    Sub AddSpell()
        Dim Input As String
        TextWriter = New StreamWriter(IncursionFolderLocation & "Data\Items\Items.txt", True)

        Console.CursorVisible = True

        Console.Clear()
        Input = "3"
        Console.Write("Name: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Required Wisdom: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Mana Cost: ")
        Input += ";" & Console.ReadLine()
        Console.Write("Magic Type (0 = Projectile, 1 = Offensive Buff, 2 = Defensive Buff, 3 = Healing, 4 = Summoning): ")
        Input += ";" & Console.ReadLine()
        Console.Write("Value (e.g. healing ammount, i.d. of npc to summon...): ")
        Input += ";" & Console.ReadLine()
        Console.Write("Cooldown Period (turns): ")
        Input += ";" & Console.ReadLine()
        Console.Write("Description: ")
        Input += ";" & Console.ReadLine()

        TextWriter.WriteLine(Input)
        TextWriter.Close()

        Console.CursorVisible = False

        Console.Clear()
        Return

    End Sub

    Sub ViewItem(ByVal ItemType)
        Dim SelectedItem As Integer = 0
        Dim OldNum As Integer = 0
        Dim Key As Integer
        Console.Clear()
        Select Case ItemType
            Case 0
                For C = 0 To WeaponTableSize
                    Console.WriteLine(WeaponTable(0, C))
                Next
            Case 1
                For C = 0 To ArmourTableSize
                    Console.WriteLine(ArmourTable(0, C))
                Next
            Case 2
                For C = 0 To ConsumableTableSize
                    Console.WriteLine(ConsumableTable(0, C))
                Next
            Case 3
                For C = 0 To SpellTableSize
                    Console.WriteLine(SpellTable(0, C))
                Next
        End Select
        While True
            Console.SetCursorPosition(0, OldNum)
            Console.Write("                                     ")
            Console.SetCursorPosition(0, OldNum)
            Select Case ItemType
                Case 0
                    Console.Write(WeaponTable(0, OldNum))
                Case 1
                    Console.Write(ArmourTable(0, OldNum))
                Case 2
                    Console.Write(ConsumableTable(0, OldNum))
                Case 3
                    Console.Write(SpellTable(0, OldNum))
            End Select
            OldNum = SelectedItem
            Console.SetCursorPosition(0, SelectedItem)
            Console.BackgroundColor = ConsoleColor.White
            Console.ForegroundColor = ConsoleColor.Black
            Select Case ItemType
                Case 0
                    Console.Write(WeaponTable(0, SelectedItem))
                Case 1
                    Console.Write(ArmourTable(0, SelectedItem))
                Case 2
                    Console.Write(ConsumableTable(0, SelectedItem))
                Case 3
                    Console.Write(SpellTable(0, SelectedItem))
            End Select
            Console.BackgroundColor = ConsoleColor.Black
            Console.ForegroundColor = ConsoleColor.White
            Key = Console.ReadKey(True).Key

            Select Case Key

                Case 38         'Down Arrow
                    If SelectedItem > 0 Then
                        SelectedItem -= 1
                    End If
                Case 40         'Up Arrow
                    Select Case ItemType
                        Case 0
                            If SelectedItem < WeaponTableSize Then
                                SelectedItem += 1
                            End If
                        Case 1
                            If SelectedItem < ArmourTableSize Then
                                SelectedItem += 1
                            End If
                        Case 2
                            If SelectedItem < ConsumableTableSize Then
                                SelectedItem += 1
                            End If
                        Case 3
                            If SelectedItem < SpellTableSize Then
                                SelectedItem += 1
                            End If
                    End Select
                Case 13         'Enter
                    Console.Clear()
                    Select Case ItemType
                        Case 0
                            Console.WriteLine("Name: " & WeaponTable(0, SelectedItem))
                            Console.WriteLine("Required Strength: " & WeaponTable(1, SelectedItem))
                            Console.WriteLine("Attack Bonus: " & WeaponTable(2, SelectedItem))
                            Console.WriteLine("1 or 2 Handed: " & WeaponTable(3, SelectedItem) & " Handed")
                            Console.WriteLine("Weight: " & WeaponTable(4, SelectedItem) & "Kg")
                            Console.WriteLine("Value: §" & WeaponTable(5, SelectedItem))
                            Console.WriteLine("Description: " & WeaponTable(6, SelectedItem))
                        Case 1
                            Console.WriteLine("Name: " & ArmourTable(0, SelectedItem))
                            Console.WriteLine("Required Strength: " & ArmourTable(1, SelectedItem))
                            Console.WriteLine("Defence Bonus: " & ArmourTable(2, SelectedItem))
                            Console.Write("Equip Location: ")
                            Select Case ArmourTable(3, SelectedItem)
                                Case 0
                                    Console.WriteLine("Head")
                                Case 1
                                    Console.WriteLine("Torso")
                                Case 2
                                    Console.WriteLine("Legs")
                                Case 3
                                    Console.WriteLine("Arms")
                                Case 4
                                    Console.WriteLine("Shield")
                            End Select
                            Console.WriteLine("Weight: " & ArmourTable(4, SelectedItem) & "Kg")
                            Console.WriteLine("Value: §" & ArmourTable(5, SelectedItem))
                            Console.WriteLine("Description: " & ArmourTable(6, SelectedItem))
                        Case 2
                            Console.WriteLine("Name: " & ConsumableTable(0, SelectedItem))
                            Console.WriteLine("Health Bonus: " & ConsumableTable(1, SelectedItem))
                            Console.WriteLine("Mana Bonus: " & ConsumableTable(2, SelectedItem))
                            Console.WriteLine("Hunger Bonus: " & ConsumableTable(3, SelectedItem))
                            Console.WriteLine("Weight: " & ConsumableTable(4, SelectedItem) & "Kg")
                            Console.WriteLine("Value: §" & ConsumableTable(5, SelectedItem))
                            Console.WriteLine("Description: " & ConsumableTable(6, SelectedItem))
                        Case 3
                            Console.WriteLine("Name: " & SpellTable(0, SelectedItem))
                            Console.WriteLine("Required Wisdom: " & SpellTable(1, SelectedItem))
                            Console.WriteLine("Mana Cost: " & SpellTable(2, SelectedItem))
                            Console.Write("Magic Type: ")
                            Select Case SpellTable(3, SelectedItem)
                                Case 0
                                    Console.WriteLine("Projectile")
                                Case 1
                                    Console.WriteLine("Offensive Buff")
                                Case 2
                                    Console.WriteLine("Defensive Buff")
                                Case 3
                                    Console.WriteLine("Healing")
                                Case 4
                                    Console.WriteLine("Summoning")
                            End Select
                            Console.WriteLine("Value: " & SpellTable(4, SelectedItem))
                            Console.WriteLine("Cooldown Period: " & SpellTable(5, SelectedItem) & " Turns")
                            Console.WriteLine("Description: " & SpellTable(6, SelectedItem))
                    End Select
                    Console.ReadLine()
                    Call ViewItem(ItemType)
                Case 27
                    Return
            End Select
        End While
        Console.ReadLine()
        Console.Clear()
    End Sub

    Sub LoadItems()

        Dim Items As String                 'String containing the entirety of the items file.
        Dim ItemContainer() As String       'Array holding each item entry
        Dim SubItemContainer(8) As String   'Array holding the further parameters of each item.
        Dim Count As Integer = -1
        Dim ItemType As Integer             'The type of the item.

        WeaponTableSize = -1
        ArmourTableSize = -1
        ConsumableTableSize = -1


        'TextReader = New StreamReader(IncursionFolderLocation & "Data\Items\Items.txt")
        'Items = TextReader.ReadToEnd
        Items = My.Resources.Items

            ItemContainer = Items.Split(vbCr)       'Loads each item entry into an array

            While True
                Try
                    Count += 1
                    SubItemContainer = ItemContainer(Count).Split(";")  'Takes an item entry in the array and splits it up further into its constituent parts
                    ItemType = SubItemContainer(0).Trim()
                    Select Case ItemType              'Work out what type of item it is
                        Case 0  'Weapon
                            ReDim Preserve WeaponTable(6, WeaponTableSize + 1)
                            WeaponTable(0, WeaponTableSize + 1) = SubItemContainer(1)
                            WeaponTable(1, WeaponTableSize + 1) = SubItemContainer(2)
                            WeaponTable(2, WeaponTableSize + 1) = SubItemContainer(3)
                            WeaponTable(3, WeaponTableSize + 1) = SubItemContainer(4)
                            WeaponTable(4, WeaponTableSize + 1) = SubItemContainer(5)
                            WeaponTable(5, WeaponTableSize + 1) = SubItemContainer(6)
                            WeaponTable(6, WeaponTableSize + 1) = SubItemContainer(7)
                            WeaponTableSize += 1
                        Case 1  'Armour
                            ReDim Preserve ArmourTable(6, ArmourTableSize + 1)
                            ArmourTable(0, ArmourTableSize + 1) = SubItemContainer(1)
                            ArmourTable(1, ArmourTableSize + 1) = SubItemContainer(2)
                            ArmourTable(2, ArmourTableSize + 1) = SubItemContainer(3)
                            ArmourTable(3, ArmourTableSize + 1) = SubItemContainer(4)
                            ArmourTable(4, ArmourTableSize + 1) = SubItemContainer(5)
                            ArmourTable(5, ArmourTableSize + 1) = SubItemContainer(6)
                            ArmourTable(6, ArmourTableSize + 1) = SubItemContainer(7)
                            ArmourTableSize += 1
                        Case 2  'Consumable
                            ReDim Preserve ConsumableTable(6, ConsumableTableSize + 1)
                            ConsumableTable(0, ConsumableTableSize + 1) = SubItemContainer(1)
                            ConsumableTable(1, ConsumableTableSize + 1) = SubItemContainer(2)
                            ConsumableTable(2, ConsumableTableSize + 1) = SubItemContainer(3)
                            ConsumableTable(3, ConsumableTableSize + 1) = SubItemContainer(4)
                            ConsumableTable(4, ConsumableTableSize + 1) = SubItemContainer(5)
                            ConsumableTable(5, ConsumableTableSize + 1) = SubItemContainer(6)
                            ConsumableTable(6, ConsumableTableSize + 1) = SubItemContainer(7)
                            ConsumableTableSize += 1
                        Case 3  'Spell
                            ReDim Preserve SpellTable(6, SpellTableSize + 1)
                            SpellTable(0, SpellTableSize + 1) = SubItemContainer(1)
                            SpellTable(1, SpellTableSize + 1) = SubItemContainer(2)
                            SpellTable(2, SpellTableSize + 1) = SubItemContainer(3)
                            SpellTable(3, SpellTableSize + 1) = SubItemContainer(4)
                            SpellTable(4, SpellTableSize + 1) = SubItemContainer(5)
                            SpellTable(5, SpellTableSize + 1) = SubItemContainer(6)
                            SpellTable(6, SpellTableSize + 1) = SubItemContainer(7)
                            SpellTableSize += 1
                    End Select
                Catch ex As Exception
                    Console.WriteLine(Count & " Item(s) Loaded.")
                    Exit While
                End Try
                Select Case ItemType
                    Case 0
                        Console.WriteLine("Loaded Weapon: " & SubItemContainer(1))      'TESTING
                    Case 1
                        Console.WriteLine("Loaded Armour: " & SubItemContainer(1))      'TESTING
                    Case 2
                        Console.WriteLine("Loaded Consumable: " & SubItemContainer(1))  'TESTING
                    Case 3
                        Console.WriteLine("Loaded Spell: " & SubItemContainer(1))
                End Select
            End While

        'TextReader.Close()
            Console.ReadLine()
    End Sub

    Sub LoadTerrain()
        Dim EntryContainer() As String      'Holds all of the lookup entries from the text file.
        Dim SingleContainer(2) As String    'Holds the parameters of each item from the entry container.
        Dim Count As Integer = -1

        EntryContainer = My.Resources.Terrain.Split(";")

        While True
            Try
                Count += 1
                ReDim Preserve TerrainLookupTable(2, Count)
                SingleContainer = EntryContainer(Count).Split("/")
                TerrainLookupTable(0, Count) = SingleContainer(0)
                TerrainLookupTable(1, Count) = SingleContainer(1)
                TerrainLookupTable(2, Count) = SingleContainer(2)
            Catch ex As Exception
                Exit While
            End Try
        End While
    End Sub

    Sub GetDrives(ByRef ValidDrives() As String, ByRef NumOfDrives As Integer)

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

    Function GetDungeonInfo(ByVal FolderID As Integer)
        Dim FileString As String
        CurrentFile = New FileStream(IncursionFolderLocation & "Data\Maps\MAP" & FolderID & "\Data.bin", FileMode.Open)
        BinaryReader = New BinaryReader(CurrentFile)

            Try
                FileString = BinaryReader.ReadString
            Catch ex As Exception
            End Try
        BinaryReader.Close()

        GetDungeonInfo = FileString

    End Function

    Sub LoadMap(ByVal FolderID As Integer, ByVal LevelID As Integer)
        Dim SolidMapString As String
        Dim SolidMapTransferArray(2559) As String
        Dim TerrainMapString As String
        Dim TerrainMapTransferArray(2559) As String
        Dim VisibilityMapString As String
        Dim VisibilityMapTransferArray(2559) As String
        Dim ItemMapString As String
        Dim ItemMapTransferArray() As String
        Dim StairString As String
        Dim StairTransferArray() As String
        Dim CurrentFilePath As String
        Dim TempChar As Char
        Dim TempCount As Integer

        Dim MapCode As Integer
        Dim Container() As String
        Dim ContainerString As String

        Dim Temp1 As Integer
        Dim Temp2 As Integer

        CurrentFile = New FileStream(IncursionFolderLocation & "Data\Maps\MAP" & FolderID & "\Data.bin", FileMode.Open)
        BinaryReader = New BinaryReader(CurrentFile)
            Try
                ContainerString = BinaryReader.ReadString
            Catch ex As Exception
            End Try
        BinaryReader.Close()
        Container = ContainerString.Split(";")

        CurrentDungeonName = Container(0)

        CurrentFilePath = IncursionFolderLocation & "Data\Maps\MAP" & FolderID & "\" & LevelID

        CurrentFile = New FileStream(CurrentFilePath, FileMode.Open)
        BinaryReader = New BinaryReader(CurrentFile)

        While True
            Try
                TempChar = BinaryReader.ReadChar
                If TempChar = "~" Then
                    Exit While
                End If
                SolidMapString = SolidMapString + TempChar
            Catch ex As Exception
            End Try
        End While

        While True
            Try
                TempChar = BinaryReader.ReadChar
                If TempChar = "~" Then
                    Exit While
                End If
                TerrainMapString = TerrainMapString + TempChar
            Catch ex As Exception
            End Try
        End While

        While True
            Try
                TempChar = BinaryReader.ReadChar
                If TempChar = "~" Then
                    Exit While
                End If
                VisibilityMapString = VisibilityMapString + TempChar
            Catch ex As Exception
            End Try
        End While

        While True
            Try
                TempChar = BinaryReader.ReadChar
                If TempChar = "~" Then
                    Exit While
                End If
                StairString = StairString + TempChar
            Catch ex As Exception
            End Try
        End While

        BinaryReader.Close()
        CurrentFile.Close()

        SolidMapTransferArray = SolidMapString.Split(",")
        SolidMapTransferArray(0) = Microsoft.VisualBasic.Right(SolidMapTransferArray(0), 1)
        TerrainMapTransferArray = TerrainMapString.Split(",")
        VisibilityMapTransferArray = VisibilityMapString.Split(",")
        StairTransferArray = StairString.Split(",")

        TempCount = 0

        For Y = 0 To 39
            For X = 0 To 63
                SolidMap(X, Y) = SolidMapTransferArray(TempCount)
                TempCount += 1
            Next
        Next

        TempCount = 0

        For Y = 0 To 39
            For X = 0 To 63
                TerrainMap(X, Y) = TerrainMapTransferArray(TempCount)
                TempCount += 1
            Next
        Next

        TempCount = 0

        For Y = 0 To 39
            For X = 0 To 63
                VisibilityMap(X, Y) = VisibilityMapTransferArray(TempCount)
                TempCount += 1
            Next
        Next

        TempCount = 0

        For Y = 0 To 39
            For X = 0 To 63
                SaveVisibilityMap(X, Y) = VisibilityMapTransferArray(TempCount)
                TempCount += 1
            Next
        Next

        UpStair(0) = StairTransferArray(0)
        UpStair(1) = StairTransferArray(1)
        DownStair(0) = StairTransferArray(2)
        DownStair(1) = StairTransferArray(3)

        For X = 0 To 63
            For Y = 0 To 39
                Temp1 += SolidMap(X, Y)
            Next
        Next

        For X = 0 To 63
            For Y = 0 To 39
                Temp2 += TerrainMap(X, Y)
            Next
        Next

        If CInt(((Temp1 * Temp2) / 7)) = Container(5 + LevelID) Then
            'Everything checks out OK!
        Else
            Console.WriteLine()
            Console.WriteLine((Temp1 * Temp2) / 7)
            Console.WriteLine(Container(5 + LevelID))
            Call CorruptedFile(FolderID, LevelID)
        End If
    End Sub

    Sub SaveMap(ByVal FolderID As Integer, ByVal LevelID As Integer)
        Dim SolidMapString As String
        Dim TerrainMapString As String
        Dim VisibilityMapString As String
        Dim Stairs As String
        Dim ItemMapString As String
        Dim CurrentFilePath As String

        Stairs = UpStair(0) & "," & UpStair(1) & "," & DownStair(0) & "," & DownStair(1) & "~"

        For Y As Integer = 0 To 39
            For X As Integer = 0 To 63
                SolidMapString += (SolidMap(X, Y) & ",")
            Next
        Next

        SolidMapString += "~"

        For Y As Integer = 0 To 39
            For X As Integer = 0 To 63
                TerrainMapString += (TerrainMap(X, Y) & ",")
            Next
        Next

        TerrainMapString += "~"

        For Y As Integer = 0 To 39
            For X As Integer = 0 To 63
                VisibilityMapString += (SaveVisibilityMap(X, Y) & ",")
            Next
        Next

        VisibilityMapString += "~"

        'For Y As Integer = 0 To 39
        'For X As Integer = 0 To 63
        'ItemMapString += (SolidMap(X, Y) & ",")
        'Next
        'Next

        'ItemMapString += "~"

        CurrentFilePath = IncursionFolderLocation & "Data\Maps\MAP" & FolderID & "\"

        If My.Computer.FileSystem.DirectoryExists(IncursionFolderLocation & "Data\Maps\MAP" & FolderID) Then
        Else
            My.Computer.FileSystem.CreateDirectory(IncursionFolderLocation & "Data\Maps\MAP" & FolderID)
        End If

        If File.Exists(CurrentFilePath & LevelID) Then
            File.Delete(CurrentFilePath & LevelID)
            File.Create(CurrentFilePath & LevelID).Dispose()
        Else
            File.Create(CurrentFilePath & LevelID).Dispose()
        End If

        CurrentFile = New FileStream(CurrentFilePath & LevelID, FileMode.Open)
        BinaryWriter = New BinaryWriter(CurrentFile)
        BinaryWriter.Write(SolidMapString & TerrainMapString & VisibilityMapString & Stairs)
        BinaryWriter.Close()

        CurrentFile.Close()
    End Sub

    Sub SavePlayerData(ByVal PlayerID As Integer)
        Dim StringToSave As String
        CurrentFile = New FileStream(IncursionFolderLocation & "\Data\Player\PlayerData.bin", FileMode.Open)
        BinaryWriter = New BinaryWriter(CurrentFile)

        StringToSave = Player.PosX
        StringToSave += ";" & Player.PosY

        StringToSave += ";" & Player.Fort1Cleared
        StringToSave += ";" & Player.Fort2Cleared
        StringToSave += ";" & Player.Fort3Cleared
        StringToSave += ";" & Player.Fort4Cleared

        StringToSave += ";" & Player.Name
        StringToSave += ";" & Player.Gender
        StringToSave += ";" & Player.Role
        StringToSave += ";" & Player.Health
        StringToSave += ";" & Player.Hunger
        StringToSave += ";" & Player.Mana
        StringToSave += ";" & Player.Armour
        StringToSave += ";" & Player.EquipmentLoad
        StringToSave += ";" & Player.MaxEquipmentLoad
        StringToSave += ";" & Player.CarryWeight
        StringToSave += ";" & Player.TimeAlive
        StringToSave += ";" & Player.KillCount

        StringToSave += ";" & Player.Level
        StringToSave += ";" & Player.Experience(0)
        StringToSave += ";" & Player.Experience(1)
        StringToSave += ";" & Player.MaxHealth
        StringToSave += ";" & Player.MaxMana
        StringToSave += ";" & Player.Strength
        StringToSave += ";" & Player.Toughness
        StringToSave += ";" & Player.RangedWeapons
        StringToSave += ";" & Player.MeleeWeapons
        StringToSave += ";" & Player.Wisdom
        StringToSave += ";" & Player.Medicine
        StringToSave += ";" & Player.Bartering

        If Player.Equipment(0) <> Nothing Then
            StringToSave += ";" & Player.Equipment(0)
        Else
            StringToSave += ";N"
        End If
        If Player.Equipment(1) <> Nothing Then
            StringToSave += ";" & Player.Equipment(1)
        Else
            StringToSave += ";N"
        End If
        If Player.Equipment(2) <> Nothing Then
            StringToSave += ";" & Player.Equipment(2)
        Else
            StringToSave += ";N"
        End If
        If Player.Equipment(3) <> Nothing Then
            StringToSave += ";" & Player.Equipment(3)
        Else
            StringToSave += ";N"
        End If
        If Player.Equipment(4) <> Nothing Then
            StringToSave += ";" & Player.Equipment(4)
        Else
            StringToSave += ";N"
        End If

        StringToSave += ";" & Player.InventorySize
        For C = 0 To Player.InventorySize
            StringToSave += ";" & Player.Inventory(0, C)
            StringToSave += "," & Player.Inventory(1, C)
            StringToSave += "," & Player.Inventory(2, C)
        Next

        StringToSave += ";" & Player.EquippedWeapon
        StringToSave += ";" & Player.Ranged
        StringToSave += ";" & Player.Arrows
        StringToSave += ";" & Player.Gold
        StringToSave += ";" & Player.NewlyEnteredDungeon

        StringToSave += ";" & Player.LastTimeHungerUpdated


    End Sub

    Sub LoadPlayerData(ByVal PlayerID As Integer)

    End Sub

    Sub SaveWorld()

    End Sub

    Sub LoadWorld()

    End Sub

    Sub CorruptedFile(ByVal FolderID As Integer, ByVal LevelID As Integer)
        Console.ForegroundColor = ConsoleColor.White
        Console.BackgroundColor = ConsoleColor.Black

        Console.WriteLine("It seems that the following file: ")
        Console.WriteLine(IncursionFolderLocation & "\Data\Maps\MAP" & FolderID & "\" & LevelID & ".bin")
        Console.WriteLine("Is corrupt. Either you modified the file, or something else has occurred.")
        Console.WriteLine("Press ENTER to delete your save data and start again.")
        Console.ReadLine()

        'Call DeleteAll()
    End Sub

    Sub DeleteAllPlayerData()
        Try
            My.Computer.FileSystem.DeleteDirectory(IncursionFolderLocation & "\Data\Maps\MAP0", FileIO.DeleteDirectoryOption.DeleteAllContents)
        Catch ex As Exception

        End Try

        Try
            My.Computer.FileSystem.DeleteDirectory(IncursionFolderLocation & "\Data\Maps\MAP1", FileIO.DeleteDirectoryOption.DeleteAllContents)
        Catch ex As Exception

        End Try

        Try
            My.Computer.FileSystem.DeleteDirectory(IncursionFolderLocation & "\Data\Maps\MAP2", FileIO.DeleteDirectoryOption.DeleteAllContents)
        Catch ex As Exception

        End Try

        Try
            My.Computer.FileSystem.DeleteDirectory(IncursionFolderLocation & "\Data\Maps\MAP3", FileIO.DeleteDirectoryOption.DeleteAllContents)
        Catch ex As Exception

        End Try

        File.Delete(IncursionFolderLocation & "\Data\Player\PlayerData.bin")

        File.Create(IncursionFolderLocation & "\Data\Player\PlayerData.bin")
    End Sub

    Sub SaveGame(ByVal Overworld As Boolean)
        Dim SaveString As String

        File.Create(IncursionFolderLocation & "Data\Player\Save.bin")

        If Overworld = True Then
            SaveString = 1
        Else
            SaveString = 0
        End If
    End Sub

#End Region

#Region "Terrain Generation"

    Function MapGen()

        Dim RoomNum As Integer                      'The number of rooms to be generated
        Dim InitialRoomNum As Integer               'This variable copies the value of RoomNum before operations are performed on the RoomNum variable
        Dim CurrentRoomCenterX As Integer           'The X position of the centre of the current room being generated
        Dim CurrentRoomCenterY As Integer           'The Y position of the centre of the current room being generated
        Dim RoomWidth As Integer                    'The width of the current room being generated
        Dim RoomHeight As Integer                   'The height of the current room being generated
        Dim PosX As Integer                         'The current X position in the map being manipulated
        Dim PosY As Integer                         'The current Y position in the map being manipulated
        Dim Rand As Integer

        Dim Temp1 As Integer
        Dim Temp2 As Integer

        Console.BackgroundColor = ConsoleColor.Black

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

        For count1 As Integer = 0 To 63
            For count2 As Integer = 0 To 39
                RoomMap(count1, count2) = 0
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
            RoomWidth = CInt((6 - 4) * Rnd()) + 3
            RoomHeight = CInt((6 - 4) * Rnd()) + 3

            PosX = CurrentRoomCenterX - CInt(RoomWidth / 2)
            PosY = CurrentRoomCenterY - CInt(RoomHeight / 2)

            If CurrentRoomCenterX + CInt(RoomWidth / 2) < 62 Then                   'Checks that the room will not be out of bounds by clipping the 'Right' of the map
                If CurrentRoomCenterX - CInt(RoomWidth / 2) > 1 Then                'Checks that the room will not be out of bounds by clipping the 'Left' of the map
                    If CurrentRoomCenterY + CInt(RoomHeight / 2) < 38 Then          'Checks that the room will not be out of bounds by clipping the 'Bottom' of the map  
                        If CurrentRoomCenterY - CInt(RoomHeight / 2) > 1 Then       'Checks that the room will not be out of bounds by clipping the 'Top' of the map


                            For Count4 As Integer = 0 To RoomHeight                                     'While the room has not had all vertical lines drawn
                                For count3 As Integer = 0 To RoomWidth                                  'For 0 to the desired length of the room
                                    SolidMap(PosX, PosY) = 0
                                    RoomMap(PosX, PosY) = 2
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
                    Call TerrainCheck(PosX, PosY)
                End If
            Next
        Next

        Rand = CInt((InitialRoomNum - 1) * Rnd())

        PosX = RoomCentres(0, Rand)
        PosY = RoomCentres(1, Rand)

        TerrainMap(PosX, PosY) = 5                     'Makes the area an upwards staircase

        UpStair(0) = PosX
        UpStair(1) = PosY

        While PosX = UpStair(0) And PosY = UpStair(1)
            Rand = CInt((InitialRoomNum - 1) * Rnd())


            PosX = RoomCentres(0, Rand)
            PosY = RoomCentres(1, Rand)
        End While

        TerrainMap(PosX, PosY) = 6                     'Makes the area a downwards staircase

        DownStair(0) = PosX
        DownStair(1) = PosY

        Call CorridorGeneration(InitialRoomNum)
        Call DoorGeneration()

        For CountY As Integer = 0 To 39
            For CountX As Integer = 0 To 63
                PosX = CountX
                PosY = CountY
                If TerrainMap(PosX, PosY) = 3 Then
                    Continue For
                ElseIf TerrainMap(PosX, PosY) = 5 Then
                    Continue For
                ElseIf TerrainMap(PosX, PosY) = 6 Then
                    Continue For
                ElseIf SolidMap(PosX, PosY) = 0 Then
                    TerrainMap(PosX, PosY) = 1           'Makes the area a normal floor tile (1)
                Else
                    Call TerrainCheck(PosX, PosY)
                End If
            Next
        Next

        For X As Integer = 0 To 63
            For Y As Integer = 0 To 39
                VisibilityMap(X, Y) = 0
            Next
        Next

        For X = 0 To 63
            For Y = 0 To 39
                Temp1 += SolidMap(X, Y)
            Next
        Next

        For X = 0 To 63
            For Y = 0 To 39
                Temp2 += TerrainMap(X, Y)
            Next
        Next

        'Call GameLoop()
        MapGen = (Temp1 * Temp2) / 7
    End Function

    Sub RenderGenerateWorld()

        Console.Clear()

        Console.BackgroundColor = ConsoleColor.DarkGray
        Console.ForegroundColor = ConsoleColor.DarkGray

        For Y = 0 To 59
            Console.SetCursorPosition(0, Y)
            Console.Write("█")
            Console.SetCursorPosition(89, Y)
            Console.Write("█")
        Next

        For X = 0 To 89
            Console.SetCursorPosition(X, 0)
            Console.Write("█")
            Console.SetCursorPosition(X, 59)
            Console.Write("█")
        Next

        Console.ForegroundColor = ConsoleColor.Black

        For Y = 1 To 58
            Console.SetCursorPosition(1, Y)
            Console.Write("▒")
            Console.SetCursorPosition(88, Y)
            Console.Write("▒")
        Next

        For X = 1 To 88
            Console.SetCursorPosition(X, 1)
            Console.Write("▒")
            Console.SetCursorPosition(X, 58)
            Console.Write("▒")
        Next

    End Sub

    Sub GenerateWorld(ByVal RejectedNumber)

        WorldWidth = 63
        WorldHeight = 39

        ReDim SolidMap(WorldWidth, WorldHeight)
        ReDim WorldMap(WorldWidth, WorldHeight)

        Dim Rand As Double
        Dim LandChance As Double = 0.51
        Dim Count As Integer
        ReDim SaltWater(WorldWidth, WorldHeight)
        SaltWater(0, 0) = 1

        Randomize()

        If RejectedNumber = 0 Then
            Call RenderGenerateWorld()
        End If

        For C = 3 To 10
            Console.SetCursorPosition(3, C)
        Next
        Console.SetCursorPosition(0, 0)

        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White

        Console.SetCursorPosition(3, 3)
        Console.Write("Generating World, Please be Patient...")

        For X = 2 To WorldWidth - 2
            For Y = 2 To WorldHeight - 2
                Rand = Rnd()
                If Rand < LandChance Then
                    SolidMap(X, Y) = 1
                Else
                    SolidMap(X, Y) = 0
                End If
            Next
        Next

        For C = 0 To 3

            For X As Integer = 2 To WorldWidth - 2
                For Y As Integer = 2 To WorldHeight - 2

                    Count = 0

                    If SolidMap(X, Y) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X + 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X + 1, Y) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X + 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X - 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X - 1, Y) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X - 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If Count >= 5 Then
                        SolidMap(X, Y) = 1
                    ElseIf Count = 0 Then
                        SolidMap(X, Y) = 1
                    Else
                        SolidMap(X, Y) = 0
                    End If
                Next
            Next

            'Console.Clear()

            'For X = 0 To MapWidth
            'For Y = 0 To MapHeight
            'Console.SetCursorPosition(X, Y)
            'If SolidMap(X, Y) = 0 Then
            'Console.WriteLine(".")
            'Else
            'Console.WriteLine("#")
            'End If
            'Next
            'Next
        Next

        For C = 0 To 2
            For X As Integer = 2 To WorldWidth - 2
                For Y As Integer = 2 To WorldHeight - 2

                    Count = 0

                    If SolidMap(X, Y) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X + 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X + 1, Y) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X + 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X - 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X - 1, Y) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X - 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If SolidMap(X, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If Count >= 5 Then
                        SolidMap(X, Y) = 1
                    Else
                        SolidMap(X, Y) = 0
                    End If
                Next
            Next

            'Console.Clear()

            'For X = 0 To MapWidth
            'For Y = 0 To MapHeight
            'Console.SetCursorPosition(X, Y)
            'If SolidMap(X, Y) = 0 Then
            'Console.WriteLine(".")
            'Else
            'Console.WriteLine("#")
            'End If
            'Next
            'Next
        Next
        'Console.ReadLine()

        For X = 0 To WorldWidth
            SolidMap(X, 0) = 0
            SolidMap(X, WorldHeight) = 0
        Next

        For Y = 0 To WorldHeight
            SolidMap(0, Y) = 0
            SolidMap(WorldWidth, Y) = 0
        Next

        For X = 0 To WorldWidth
            For Y = 0 To WorldHeight
                If SolidMap(X, Y) = 0 Then
                    WorldMap(X, Y) = 7
                End If
            Next
        Next

        SaltWater(0, 0) = 1
        CheckWater()

        For X = 0 To WorldWidth
            For Y = 0 To WorldHeight
                If SolidMap(X, Y) = 0 Then
                    If SaltWater(X, Y) = 0 Then
                        WorldMap(X, Y) = 10
                    End If
                End If
            Next
        Next

        For X = 0 To WorldWidth
            For Y = 0 To WorldHeight
                Try
                    If SolidMap(X, Y) = 1 Then
                        If WorldMap(X + 1, Y + 1) <> 7 Then
                            If WorldMap(X + 1, Y) <> 7 Then
                                If WorldMap(X + 1, Y - 1) <> 7 Then
                                    If WorldMap(X, Y - 1) <> 7 Then
                                        If WorldMap(X - 1, Y - 1) <> 7 Then
                                            If WorldMap(X - 1, Y) <> 7 Then
                                                If WorldMap(X - 1, Y + 1) <> 7 Then
                                                    If WorldMap(X, Y + 1) <> 7 Then
                                                        WorldMap(X, Y) = 9
                                                        Exit Try
                                                    End If
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                        WorldMap(X, Y) = 8
                    Else
                    End If
                Catch ex As Exception
                End Try
            Next
        Next

        GenerateMountains()
        GenerateForests()
        GenerateRivers()

        While True
            For X = 0 To WorldWidth
                For Y = 0 To WorldHeight
                    If WorldMap(X, Y) = 10 Then
                        Exit While
                    End If
                Next
            Next
            RejectedNumber += 1
            Console.SetCursorPosition(3, 4)
            Console.WriteLine("Rejected " & RejectedNumber & " Worlds.")
            Call GenerateWorld(RejectedNumber)
        End While

        Call PlaceDungeons()

        Call Overworld()

    End Sub

    Sub PlaceDungeons()
        Dim TownX As Integer
        Dim TownY As Integer

        TownX = 0
        TownY = 0

        While True
            TownX = CInt(WorldWidth * Rnd())
            TownY = CInt(WorldHeight * Rnd())
            If WorldMap(TownX, TownY) = 9 Then
                Exit While
            ElseIf WorldMap(TownX, TownY) = 12 Then
                Exit While
            ElseIf WorldMap(TownX, TownY) = 11 Then
                Exit While
            End If
        End While

        WorldMap(TownX, TownY) = 27
        Console.SetCursorPosition(3, 5)
        Console.WriteLine("Generating First Fortress...")
        Console.SetCursorPosition(3, 10)
        Console.Write("                                                                    ")
        Call DungeonGen(25, 10)
        Console.SetCursorPosition(3, 6)
        Console.WriteLine("Generating Second Fortress...")
        Console.SetCursorPosition(3, 10)
        Console.Write("                                                                    ")
        Call DungeonGen(50, 30)
        Console.SetCursorPosition(3, 7)
        Console.WriteLine("Generating Third Fortress...")
        Console.SetCursorPosition(3, 10)
        Console.Write("                                                                    ")
        Call DungeonGen(75, 50)
        Console.SetCursorPosition(3, 8)
        Console.WriteLine("Generating Fourth Fortress...")
        Console.SetCursorPosition(3, 10)
        Console.Write("                                                                    ")
        Call DungeonGen(100, 100)
        Console.SetCursorPosition(3, 10)
        Console.Write("                                                          ")
        ConsoleOutput.SlowWrite(3, 12, 125, "Done!                                 ")
        System.Threading.Thread.Sleep(1000)

        OverworldX = TownX
        OverworldY = TownY

        Return
    End Sub

    Sub CheckWater()

        Dim ChangeMade As Boolean = True

        While ChangeMade = True
            ChangeMade = False
            For X = 0 To WorldWidth
                For Y = 0 To WorldHeight
                    If SaltWater(X, Y) = 1 Then

                        If X = 0 Then
                            If Y = 0 Then

                                If SolidMap(X + 1, Y) = 0 Then
                                    If SaltWater(X + 1, Y) = 0 Then
                                        SaltWater(X + 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y + 1) = 0 Then
                                    If SaltWater(X, Y + 1) = 0 Then
                                        SaltWater(X, Y + 1) = 1
                                        ChangeMade = True
                                    End If
                                End If

                            ElseIf Y = WorldHeight Then

                                If SolidMap(X + 1, Y) = 0 Then
                                    If SaltWater(X + 1, Y) = 0 Then
                                        SaltWater(X + 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y - 1) = 0 Then
                                    If SaltWater(X, Y - 1) = 0 Then
                                        SaltWater(X, Y - 1) = 1
                                        ChangeMade = True
                                    End If
                                End If

                            Else

                                If SolidMap(X + 1, Y) = 0 Then
                                    If SaltWater(X + 1, Y) = 0 Then
                                        SaltWater(X + 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y - 1) = 0 Then
                                    If SaltWater(X, Y - 1) = 0 Then
                                        SaltWater(X, Y - 1) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y + 1) = 0 Then
                                    If SaltWater(X, Y + 1) = 0 Then
                                        SaltWater(X, Y + 1) = 1
                                        ChangeMade = True
                                    End If
                                End If

                            End If

                        ElseIf X = WorldWidth Then

                            If Y = 0 Then

                                If SolidMap(X - 1, Y) = 0 Then
                                    If SaltWater(X - 1, Y) = 0 Then
                                        SaltWater(X - 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y + 1) = 0 Then
                                    If SaltWater(X, Y + 1) = 0 Then
                                        SaltWater(X, Y + 1) = 1
                                        ChangeMade = True
                                    End If
                                End If

                            ElseIf Y = WorldHeight Then

                                If SolidMap(X - 1, Y) = 0 Then
                                    If SaltWater(X - 1, Y) = 0 Then
                                        SaltWater(X - 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y - 1) = 0 Then
                                    If SaltWater(X, Y - 1) = 0 Then
                                        SaltWater(X, Y - 1) = 1
                                        ChangeMade = True
                                    End If
                                End If

                            Else

                                If SolidMap(X - 1, Y) = 0 Then
                                    If SaltWater(X - 1, Y) = 0 Then
                                        SaltWater(X - 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y - 1) = 0 Then
                                    If SaltWater(X, Y - 1) = 0 Then
                                        SaltWater(X, Y - 1) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y + 1) = 0 Then
                                    If SaltWater(X, Y + 1) = 0 Then
                                        SaltWater(X, Y + 1) = 1
                                        ChangeMade = True
                                    End If
                                End If

                            End If

                        Else
                            If Y = 0 Then

                                If SolidMap(X + 1, Y) = 0 Then
                                    If SaltWater(X + 1, Y) = 0 Then
                                        SaltWater(X + 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X - 1, Y) = 0 Then
                                    If SaltWater(X - 1, Y) = 0 Then
                                        SaltWater(X - 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y + 1) = 0 Then
                                    If SaltWater(X, Y + 1) = 0 Then
                                        SaltWater(X, Y + 1) = 1
                                        ChangeMade = True
                                    End If
                                End If
                            ElseIf Y = WorldHeight Then

                                If SolidMap(X + 1, Y) = 0 Then
                                    If SaltWater(X + 1, Y) = 0 Then
                                        SaltWater(X + 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X - 1, Y) = 0 Then
                                    If SaltWater(X - 1, Y) = 0 Then
                                        SaltWater(X - 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y - 1) = 0 Then
                                    If SaltWater(X, Y - 1) = 0 Then
                                        SaltWater(X, Y - 1) = 1
                                        ChangeMade = True
                                    End If
                                End If

                            Else

                                If SolidMap(X + 1, Y) = 0 Then
                                    If SaltWater(X + 1, Y) = 0 Then
                                        SaltWater(X + 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X - 1, Y) = 0 Then
                                    If SaltWater(X - 1, Y) = 0 Then
                                        SaltWater(X - 1, Y) = 1
                                        ChangeMade = True
                                    End If
                                End If

                                If SolidMap(X, Y + 1) = 0 Then
                                    If SaltWater(X, Y + 1) = 0 Then
                                        SaltWater(X, Y + 1) = 1
                                        ChangeMade = True
                                    End If
                                End If


                                If SolidMap(X, Y - 1) = 0 Then
                                    If SaltWater(X, Y - 1) = 0 Then
                                        SaltWater(X, Y - 1) = 1
                                        ChangeMade = True
                                    End If
                                End If

                            End If
                        End If

                    End If
                Next
            Next

        End While
    End Sub

    Sub GenerateRivers()

        Dim PosX As Integer
        Dim PosY As Integer
        Dim DestX As Integer
        Dim DestY As Integer
        Dim RiverMap(WorldWidth, WorldHeight) As Integer

        Dim CurrentX As Integer
        Dim CurrentY As Integer

        Dim StartOfRiver As Boolean = False     'Is the current square the start of the river?

        Dim OpenList(,) As Integer              'Holds all of the squares that need to be compared
        Dim ClosedList(,) As Integer            'Holds the squares that are desired.
        Dim VisitedSquares(WorldWidth, WorldHeight) As Integer  'The squares that have been added to the closed list.
        Dim F As Integer    'The total score of a square
        Dim G As Integer    'The base score (10)
        Dim H As Integer    'The distance from the current square to the target 'as the crow flies' (pythagoras).
        Dim OpenListEntryNumber As Integer      'The number of entries in the openlist
        Dim ClosedListEntryNumber As Integer    'The number of entries in the closedlist.
        Dim ParentID As Integer     'The ID of the square that came before the square currently being added to the closed list.
        Dim DesiredID As Integer    'The best square out of the current open list.
        Dim CanMove As Boolean = False  'Can the path go any further? 
        Dim NeedsRiver As Boolean = False   'Is another river needed?
        Dim RiverPath(,) As Integer         'The X and Y coordinates of each square in the rivers path.

        Dim BreakoutNumber As Integer = 6   'Limits the maximum number of rivers to 6 - prevents infinite loops.

        While True
            If BreakoutNumber = 0 Then
                Return
            End If

            SaltWater(0, 0) = 1
            Call CheckWater()

            NeedsRiver = False

            For X = 0 To WorldWidth
                For Y = 0 To WorldHeight
                    If SolidMap(X, Y) = 0 Then
                        If SaltWater(X, Y) = 0 Then
                            PosX = X
                            PosY = Y
                            NeedsRiver = True
                            Exit For
                        End If
                    End If
                Next
                If NeedsRiver = True Then
                    Exit For
                End If
            Next

            If NeedsRiver = False Then
                Return
            End If

            While True
                DestX = CInt(WorldWidth * Rnd())
                DestY = CInt(WorldHeight * Rnd())
                If SolidMap(DestX, DestY) = 0 Then
                    If WorldMap(DestX, DestY) = 7 Then
                        Exit While
                    End If
                End If
            End While

            CurrentX = PosX
            CurrentY = PosY

            For X = 0 To WorldWidth
                For Y = 0 To WorldHeight
                    VisitedSquares(X, Y) = 0
                Next
            Next

            VisitedSquares(CurrentX, CurrentY) = 1

            ReDim Preserve ClosedList(1, 0)
            ClosedList(0, 0) = CurrentX
            ClosedList(1, 0) = CurrentY

            OpenListEntryNumber = -1
            ClosedListEntryNumber = 0
            G = 10

            While ((CurrentX <> DestX) Or (CurrentY <> DestY))

                CanMove = False

                Try
                    If WorldMap(CurrentX + 1, CurrentY) <> 11 Then
                        If VisitedSquares(CurrentX + 1, CurrentY) = 0 Then
                            OpenListEntryNumber += 1
                            ReDim Preserve OpenList(3, OpenListEntryNumber)
                            OpenList(0, OpenListEntryNumber) = CurrentX + 1
                            OpenList(1, OpenListEntryNumber) = CurrentY
                            OpenList(2, OpenListEntryNumber) = ParentID
                            H = Math.Abs((CurrentX + 1) - DestX) + Math.Abs(CurrentY - DestY)
                            F = G + H
                            OpenList(3, OpenListEntryNumber) = F
                            CanMove = True
                        End If
                    End If
                Catch ex As Exception
                End Try

                Try
                    If WorldMap(CurrentX - 1, CurrentY) <> 11 Then
                        If VisitedSquares(CurrentX - 1, CurrentY) = 0 Then
                            OpenListEntryNumber += 1
                            ReDim Preserve OpenList(3, OpenListEntryNumber)
                            OpenList(0, OpenListEntryNumber) = CurrentX - 1
                            OpenList(1, OpenListEntryNumber) = CurrentY
                            OpenList(2, OpenListEntryNumber) = ParentID
                            H = Math.Abs((CurrentX - 1) - DestX) + Math.Abs(CurrentY - DestY)
                            F = G + H
                            OpenList(3, OpenListEntryNumber) = F
                            CanMove = True
                        End If
                    End If
                Catch ex As Exception
                End Try

                Try
                    If WorldMap(CurrentX, CurrentY + 1) <> 11 Then
                        If VisitedSquares(CurrentX, CurrentY + 1) = 0 Then
                            OpenListEntryNumber += 1
                            ReDim Preserve OpenList(3, OpenListEntryNumber)
                            OpenList(0, OpenListEntryNumber) = CurrentX
                            OpenList(1, OpenListEntryNumber) = CurrentY + 1
                            OpenList(2, OpenListEntryNumber) = ParentID
                            H = Math.Abs(CurrentX - DestX) + Math.Abs((CurrentY + 1) - DestY)
                            F = G + H
                            OpenList(3, OpenListEntryNumber) = F
                            CanMove = True
                        End If
                    End If
                Catch ex As Exception
                End Try

                Try
                    If WorldMap(CurrentX, CurrentY - 1) <> 11 Then
                        If VisitedSquares(CurrentX, CurrentY - 1) = 0 Then
                            OpenListEntryNumber += 1
                            ReDim Preserve OpenList(3, OpenListEntryNumber)
                            OpenList(0, OpenListEntryNumber) = CurrentX
                            OpenList(1, OpenListEntryNumber) = CurrentY - 1
                            OpenList(2, OpenListEntryNumber) = ParentID
                            H = Math.Abs(CurrentX - DestX) + Math.Abs((CurrentY - 1) - DestY)
                            F = G + H
                            OpenList(3, OpenListEntryNumber) = F
                            CanMove = True
                        End If
                    End If
                Catch ex As Exception
                End Try

                If CanMove = False Then
                    Exit While
                End If
                DesiredID = 0

                For Count = 0 To OpenListEntryNumber
                    If OpenList(3, Count) < OpenList(3, DesiredID) Then
                        DesiredID = Count
                    End If
                Next

                ClosedListEntryNumber += 1

                ReDim Preserve ClosedList(1, ClosedListEntryNumber)

                ClosedList(0, ClosedListEntryNumber) = OpenList(0, DesiredID)
                ClosedList(1, ClosedListEntryNumber) = OpenList(1, DesiredID)

                CurrentX = OpenList(0, DesiredID)
                CurrentY = OpenList(1, DesiredID)

                VisitedSquares(CurrentX, CurrentY) = 1

                OpenListEntryNumber = -1

            End While

            ReDim Preserve RiverPath(1, ClosedListEntryNumber)

            For C = 0 To ClosedListEntryNumber
                RiverPath(0, C) = ClosedList(0, C)
                RiverPath(1, C) = ClosedList(1, C)
            Next

            StartOfRiver = False

            For C = 0 To ClosedListEntryNumber
                PosX = RiverPath(0, C)
                PosY = RiverPath(1, C)

                If SolidMap(PosX, PosY) = 1 Then
                    If StartOfRiver = False Then
                        StartOfRiver = True
                        RiverMap(RiverPath(0, C - 1), RiverPath(1, C - 1)) = 1
                    End If
                    SolidMap(PosX, PosY) = 0
                    RiverMap(PosX, PosY) = 1
                Else
                    If WorldMap(PosX, PosY) = 7 Then
                        Exit For
                    End If
                End If
            Next

            BreakoutNumber -= 1

            For Y = 0 To WorldHeight
                For X = 0 To WorldWidth
                    Try
                        If ((RiverMap(X, Y) = 1) And (WorldMap(X, Y) <> 10)) Then
                            If RiverMap(X + 1, Y) = 1 Then  'East
                                If RiverMap(X - 1, Y) = 1 Then  'East, West
                                    If RiverMap(X, Y + 1) = 1 Then  'East, West, South
                                        If RiverMap(X, Y - 1) = 1 Then  'East, West, South, North
                                            WorldMap(X, Y) = 20 '┼
                                            Exit Try
                                        Else 'East, West, South | North
                                            WorldMap(X, Y) = 19 '┬
                                            Exit Try
                                        End If
                                    Else 'East, West | South
                                        If RiverMap(X, Y - 1) = 1 Then  'East, West, North | South
                                            WorldMap(X, Y) = 18 '┴
                                            Exit Try
                                        Else 'East, West | North, South
                                            WorldMap(X, Y) = 15 '─
                                            Exit Try
                                        End If
                                    End If
                                ElseIf RiverMap(X, Y + 1) = 1 Then 'East, South | West
                                    If RiverMap(X, Y - 1) = 1 Then 'East, South, North | West
                                        WorldMap(X, Y) = 17 '├
                                        Exit Try
                                    Else 'East, South | West, North
                                        WorldMap(X, Y) = 23 '┌
                                        Exit Try
                                    End If
                                ElseIf RiverMap(X, Y - 1) = 1 Then 'East, North | South, West
                                    WorldMap(X, Y) = 22 '└
                                    Exit Try
                                Else 'East | North, South, West
                                    WorldMap(X, Y) = 15 '─
                                    Exit Try
                                End If

                            ElseIf RiverMap(X - 1, Y) = 1 Then  'West | East
                                If RiverMap(X, Y - 1) = 1 Then  'West, North | East
                                    If RiverMap(X, Y + 1) = 1 Then  'West, North, South | East
                                        WorldMap(X, Y) = 16 '┤
                                        Exit Try
                                    Else    'West, North | South, East
                                        WorldMap(X, Y) = 24 '┘
                                        Exit Try
                                    End If
                                Else 'West | North, East
                                    If RiverMap(X, Y + 1) = 1 Then 'West, South | North, East
                                        WorldMap(X, Y) = 21 '┐
                                        Exit Try
                                    Else 'West | North, East, South
                                        WorldMap(X, Y) = 15 '─
                                        Exit Try
                                    End If
                                End If
                            ElseIf RiverMap(X, Y - 1) = 1 Then  'North | East, West
                                WorldMap(X, Y) = 14 '│
                                Exit Try
                            ElseIf RiverMap(X, Y + 1) = 1 Then  'South | East, West, North
                                WorldMap(X, Y) = 14 '│
                                Exit Try
                            Else

                            End If
                        End If
                    Catch ex As Exception
                    End Try
                Next
            Next

        End While
    End Sub

    Sub GenerateMountains()

        Dim Rand As Double
        Dim LandChance As Double = 0.48
        Dim Count As Integer
        Dim MountainMap(WorldWidth, WorldHeight) As Integer
        Randomize()

        For X = 2 To WorldWidth - 2
            For Y = 2 To WorldHeight - 2
                Rand = Rnd()
                If Rand < LandChance Then
                    If SolidMap(X, Y) = 1 Then
                        If WorldMap(X, Y) <> 8 Then
                            MountainMap(X, Y) = 1
                        End If
                    End If
                Else
                    MountainMap(X, Y) = 0
                End If
            Next
        Next

        For C = 0 To 3

            For X As Integer = 2 To WorldWidth - 2
                For Y As Integer = 2 To WorldHeight - 2

                    Count = 0

                    If MountainMap(X, Y) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If Count >= 5 Then
                        MountainMap(X, Y) = 1
                    ElseIf Count = 0 Then
                        MountainMap(X, Y) = 1
                    Else
                        MountainMap(X, Y) = 0
                    End If
                Next
            Next

            'Console.Clear()

        Next

        For C = 0 To 2
            For X As Integer = 2 To WorldWidth - 2
                For Y As Integer = 2 To WorldHeight - 2

                    Count = 0

                    If MountainMap(X, Y) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If MountainMap(X, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If Count >= 5 Then
                        If SolidMap(X, Y) = 1 Then
                            If WorldMap(X, Y) <> 8 Then
                                MountainMap(X, Y) = 1
                            End If
                        End If
                    Else
                        MountainMap(X, Y) = 0
                    End If
                Next
            Next

            'Console.Clear()

        Next
        'Console.ReadLine()

        For X = 0 To WorldWidth
            MountainMap(X, 0) = 0
            MountainMap(X, WorldHeight) = 0
        Next

        For Y = 0 To WorldHeight
            MountainMap(0, Y) = 0
            MountainMap(WorldWidth, Y) = 0
        Next

        For X = 0 To WorldWidth
            For Y = 0 To WorldHeight
                If MountainMap(X, Y) = 1 Then
                    WorldMap(X, Y) = 11
                End If
            Next
        Next

        'Now, generate taller mountains on top of the existing ones

        LandChance = 0.6

        For X = 2 To WorldWidth - 2
            For Y = 2 To WorldHeight - 2
                Rand = Rnd()
                If Rand < LandChance Then
                    If MountainMap(X, Y) = 1 Then
                        If WorldMap(X, Y) <> 8 Then
                            MountainMap(X, Y) = 2
                        End If
                    End If
                Else
                End If
            Next
        Next

        For C = 0 To 3

            For X As Integer = 2 To WorldWidth - 2
                For Y As Integer = 2 To WorldHeight - 2

                    Count = 0

                    If MountainMap(X, Y) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y + 1) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y - 1) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X, Y - 1) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y - 1) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y + 1) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X, Y + 1) = 2 Then
                        Count += 1
                    End If

                    If Count >= 5 Then
                        MountainMap(X, Y) = 2
                    ElseIf Count = 0 Then
                        MountainMap(X, Y) = 2
                    Else
                    End If
                Next
            Next

            'Console.Clear()

        Next

        For C = 0 To 2
            For X As Integer = 2 To WorldWidth - 2
                For Y As Integer = 2 To WorldHeight - 2

                    Count = 0

                    If MountainMap(X, Y) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y + 1) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X + 1, Y - 1) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X, Y - 1) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y - 1) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X - 1, Y + 1) = 2 Then
                        Count += 1
                    End If

                    If MountainMap(X, Y + 1) = 2 Then
                        Count += 1
                    End If

                    If Count >= 5 Then
                        If SolidMap(X, Y) = 1 Then
                            If MountainMap(X, Y) = 1 Then
                                MountainMap(X, Y) = 2
                            End If
                        End If
                    Else
                    End If
                Next
            Next

            'Console.Clear()

        Next
        'Console.ReadLine()

        For X = 0 To WorldWidth
            MountainMap(X, 0) = 0
            MountainMap(X, WorldHeight) = 0
        Next

        For Y = 0 To WorldHeight
            MountainMap(0, Y) = 0
            MountainMap(WorldWidth, Y) = 0
        Next

        For X = 0 To WorldWidth
            For Y = 0 To WorldHeight
                If MountainMap(X, Y) = 1 Then
                    WorldMap(X, Y) = 25
                End If
            Next
        Next
    End Sub

    Sub GenerateForests()

        Dim Rand As Double
        Dim LandChance As Double = 0.6
        Dim Count As Integer
        Dim ForestMap(WorldWidth, WorldHeight) As Integer
        Randomize()

        For X = 2 To WorldWidth - 2
            For Y = 2 To WorldHeight - 2
                Rand = Rnd()
                If Rand < LandChance Then
                    If SolidMap(X, Y) = 1 Then
                        If WorldMap(X, Y) <> 8 Then
                            If WorldMap(X, Y) <> 11 Then
                                ForestMap(X, Y) = 1
                            End If
                        End If
                    End If
                Else
                    ForestMap(X, Y) = 0
                End If
            Next
        Next

        For C = 0 To 3

            For X As Integer = 2 To WorldWidth - 2
                For Y As Integer = 2 To WorldHeight - 2

                    Count = 0

                    If ForestMap(X, Y) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X + 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X + 1, Y) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X + 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X - 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X - 1, Y) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X - 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If Count >= 5 Then
                        If SolidMap(X, Y) = 1 Then
                            If WorldMap(X, Y) <> 8 Then
                                If WorldMap(X, Y) <> 11 Then
                                    ForestMap(X, Y) = 1
                                End If
                            End If
                        End If
                    ElseIf Count = 0 Then
                        If SolidMap(X, Y) = 1 Then
                            If WorldMap(X, Y) <> 8 Then
                                If WorldMap(X, Y) <> 11 Then
                                    ForestMap(X, Y) = 1
                                End If
                            End If
                        End If
                    Else
                        ForestMap(X, Y) = 0
                    End If
                Next
            Next

            'Console.Clear()

        Next

        For C = 0 To 2
            For X As Integer = 2 To WorldWidth - 2
                For Y As Integer = 2 To WorldHeight - 2

                    Count = 0

                    If ForestMap(X, Y) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X + 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X + 1, Y) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X + 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X - 1, Y - 1) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X - 1, Y) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X - 1, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If ForestMap(X, Y + 1) = 1 Then
                        Count += 1
                    End If

                    If Count >= 5 Then
                        If SolidMap(X, Y) = 1 Then
                            If WorldMap(X, Y) <> 8 Then
                                If WorldMap(X, Y) <> 11 Then
                                    ForestMap(X, Y) = 1
                                End If
                            End If
                        End If
                    Else
                        ForestMap(X, Y) = 0
                    End If
                Next
            Next

            'Console.Clear()

        Next
        'Console.ReadLine()

        For X = 0 To WorldWidth
            ForestMap(X, 0) = 0
            ForestMap(X, WorldHeight) = 0
        Next

        For Y = 0 To WorldHeight
            ForestMap(0, Y) = 0
            ForestMap(WorldWidth, Y) = 0
        Next

        For X = 0 To WorldWidth
            For Y = 0 To WorldHeight
                If ForestMap(X, Y) = 1 Then
                    WorldMap(X, Y) = 12
                End If
            Next
        Next

    End Sub

    Sub CorridorGeneration(ByVal RoomNum As Integer)


        Dim CorridorMakeable As Boolean = True
        Dim CurrentRoomCenterX As Integer
        Dim CurrentRoomCenterY As Integer
        Dim PosX As Integer
        Dim PosY As Integer
        Dim DestX As Integer
        Dim DestY As Integer

        While CorridorMakeable = True
            CorridorMakeable = False
            For X As Integer = 0 To (RoomNum - 1)                               'For each room
                CurrentRoomCenterX = RoomCentres(0, X)                          'Get the X position of the room's center
                CurrentRoomCenterY = RoomCentres(1, X)                          'Get the Y position of the room's center

                Call FloodFill(UpStair(0), UpStair(1))

                If FloodMap(CurrentRoomCenterX, CurrentRoomCenterY) = 1 Then
                Else
                    CorridorMakeable = True
                    PosX = CInt((62 - 2) * Rnd())
                    PosY = CInt((38 - 2) * Rnd())
                    While FloodMap(PosX, PosY) <> 1
                        PosX = CInt((62 - 2) * Rnd())
                        PosY = CInt((38 - 2) * Rnd())
                    End While
                    DestX = CurrentRoomCenterX
                    DestY = CurrentRoomCenterY

                    While PosX <> DestX
                        While PosY <> DestY
                            If DestY > PosY Then
                                PosY += 1
                            ElseIf DestY < PosY Then
                                PosY -= 1
                            End If
                            SolidMap(PosX, PosY) = 0
                            RoomMap(PosX, PosY) = 1
                        End While
                        If DestX > PosX Then
                            PosX += 1
                        ElseIf DestX < PosX Then
                            PosX -= 1
                        End If
                        SolidMap(PosX, PosY) = 0
                        RoomMap(PosX, PosY) = 1
                    End While
                    Call FloodFill(UpStair(0), UpStair(1))
                End If

            Next
        End While
    End Sub

    Sub DoorGeneration()
        Dim X As Integer
        Dim Y As Integer
        Dim C As Integer
        Dim Breakout As Boolean = False
        Dim WallCheck As Boolean

        While Breakout = False
            Breakout = True
            For X = 0 To 63
                For Y = 0 To 39
                    If RoomMap(X, Y) = 1 Then
                        C = 0
                        If RoomMap(X, Y + 1) = 2 Then
                            C += 1
                        End If
                        If RoomMap(X + 1, Y + 1) = 2 Then
                            C += 1
                        End If
                        If RoomMap(X + 1, Y) = 2 Then
                            C += 1
                        End If
                        If RoomMap(X + 1, Y - 1) = 2 Then
                            C += 1
                        End If
                        If RoomMap(X, Y - 1) = 2 Then
                            C += 1
                        End If
                        If RoomMap(X - 1, Y - 1) = 2 Then
                            C += 1
                        End If
                        If RoomMap(X - 1, Y) = 2 Then
                            C += 1
                        End If
                        If RoomMap(X - 1, Y + 1) = 2 Then
                            C += 1
                        End If
                        If C > 2 Then
                            RoomMap(X, Y) = 2
                            Breakout = False
                        End If
                    End If
                Next
            Next

        End While

        For X = 0 To 63
            For Y = 0 To 39
                If RoomMap(X, Y) = 1 Then
                    WallCheck = False
                    If RoomMap(X + 1, Y) = 0 Then
                        If RoomMap(X - 1, Y) = 0 Then
                            WallCheck = True
                        End If
                    ElseIf RoomMap(X, Y + 1) = 0 Then
                        If RoomMap(X, Y - 1) = 0 Then
                            WallCheck = True
                        End If
                    End If

                    If WallCheck = True Then
                        If RoomMap(X + 1, Y) = 2 Then
                            If RoomMap(X - 1, Y) = 1 Then
                                SolidMap(X, Y) = 2
                                TerrainMap(X, Y) = 3
                            End If
                        End If
                        If RoomMap(X - 1, Y) = 2 Then
                            If RoomMap(X + 1, Y) = 1 Then
                                SolidMap(X, Y) = 2
                                TerrainMap(X, Y) = 3
                            End If
                        End If
                        If RoomMap(X, Y + 1) = 2 Then
                            If RoomMap(X, Y - 1) = 1 Then
                                SolidMap(X, Y) = 2
                                TerrainMap(X, Y) = 3
                            End If
                        End If
                        If RoomMap(X, Y - 1) = 2 Then
                            If RoomMap(X, Y + 1) = 1 Then
                                SolidMap(X, Y) = 2
                                TerrainMap(X, Y) = 3
                            End If
                        End If
                    End If

                End If
            Next
        Next

    End Sub

    Sub DungeonGen(ByVal NumberOfFloors As Integer, ByVal MaxEnemyLevel As Integer)
        Dim FolderCount As Integer = 0
        Dim LevelModifier As Double     'The level modifier is what each floor is multiplied by to give the maximum enenmy level for that floor. For example, if I was on floor 22 with a level modifier of 0.8, I could expect to see monsters at level 18 (rounded up)
        Dim DungeonName As String       'The name of the dungeon.
        Dim FolderID As Integer
        Dim CurrentDirectory As String
        Dim LevelID As Integer
        Dim Temp As Integer
        Dim DungeonX As Integer
        Dim DungeonY As Integer
        Dim StringToSave As String

        Dim CurrentProgress As Double
        Dim BlockIncreaseAmmount As Double

        Dim MapCode As Integer

        LevelModifier = MaxEnemyLevel / NumberOfFloors      'This calculation means that at the bottom floor the enemies will be at the max level.
        BlockIncreaseAmmount = 58 / (NumberOfFloors - 1)

        DungeonName = NameGeneration()

        DungeonX = 0
        DungeonY = 0

        While True
            DungeonX = CInt(WorldWidth * Rnd())
            DungeonY = CInt(WorldHeight * Rnd())
            If WorldMap(DungeonX, DungeonY) = 9 Then
                Exit While
            ElseIf WorldMap(DungeonX, DungeonY) = 12 Then
                Exit While
            ElseIf WorldMap(DungeonX, DungeonY) = 11 Then
                Exit While
            End If
        End While

        WorldMap(DungeonX, DungeonY) = 26

        While My.Computer.FileSystem.DirectoryExists(IncursionFolderLocation & "Data\Maps\MAP" & FolderCount)
            FolderCount += 1
        End While

        FolderID = FolderCount

        CurrentDirectory = IncursionFolderLocation & "\Data\Maps\MAP" & FolderID

        My.Computer.FileSystem.CreateDirectory(CurrentDirectory)

        StringToSave = DungeonName & ";" & LevelModifier & ";" & NumberOfFloors & ";" & DungeonX & ";" & DungeonY & ";"

        CurrentProgress = 0

        Console.SetCursorPosition(3, 5)

        For LevelID = 0 To NumberOfFloors - 1

            CurrentProgress = CurrentProgress + BlockIncreaseAmmount

            For C = 0 To CInt(CurrentProgress)
                Console.SetCursorPosition(3 + C, 10)
                Console.Write("█")
            Next

            MapCode = CInt(MapGen())
            Call SaveMap(FolderID, LevelID)

            StringToSave += MapCode & ";"

        Next
        CurrentFile = New FileStream(IncursionFolderLocation & "\Data\Maps\MAP" & FolderID & "\Data.bin", FileMode.Create)
        BinaryWriter = New BinaryWriter(CurrentFile)
        BinaryWriter.Write(StringToSave)
        BinaryWriter.Close()

        Return
    End Sub

    Sub TerrainCheck(ByVal PosX As Integer, ByVal PosY As Integer)

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

    Function NameGeneration()
        Dim Temp As Integer

        Temp = CInt(9 * Rnd())

        NameGeneration = "The "

        Select Case Temp
            Case 0
                NameGeneration += "Halls of "
            Case 1
                NameGeneration += "Caves of "
            Case 2
                NameGeneration += "Lost Fortress of "
            Case 3
                NameGeneration += "Dread Fortress "
            Case 4
                NameGeneration += "Castle of "
            Case 5
                NameGeneration += "Chambers of "
            Case 6
                NameGeneration += "Archives of "
            Case 7
                NameGeneration += "Fortress "
            Case 8
                NameGeneration += "Castle "
            Case 9
                NameGeneration += "Fabled Ruins of "
        End Select

        Temp = CInt(9 * Rnd())

        Select Case Temp
            Case 0
                NameGeneration += "Ast"
            Case 1
                NameGeneration += "Ber"
            Case 2
                NameGeneration += "Thul"
            Case 3
                NameGeneration += "Gra"
            Case 4
                NameGeneration += "Kle"
            Case 5
                NameGeneration += "Jo"
            Case 6
                NameGeneration += "Sen"
            Case 7
                NameGeneration += "And"
            Case 8
                NameGeneration += "Elk"
            Case 9
                NameGeneration += "Urk"
        End Select

        Temp = CInt(9 * Rnd())

        Select Case Temp
            Case 0
                NameGeneration += "ora."
            Case 1
                NameGeneration += "ulton."
            Case 2
                NameGeneration += "eve."
            Case 3
                NameGeneration += "anan."
            Case 4
                NameGeneration += "seef."
            Case 5
                NameGeneration += "krep."
            Case 6
                NameGeneration += "bruch."
            Case 7
                NameGeneration += "dorf."
            Case 8
                NameGeneration += "mount."
            Case 9
                NameGeneration += "lel."
        End Select

    End Function

    Sub FloodFill(ByVal StairX, ByVal StairY)
        Dim CurrentX As Integer
        Dim CurrentY As Integer
        Dim MoveMade As Boolean
        For X = 0 To 63
            For Y = 0 To 39
                If SolidMap(X, Y) = 1 Then
                    FloodMap(X, Y) = 2
                Else
                    FloodMap(X, Y) = 0
                End If
            Next
        Next

        CurrentX = CInt((62 - 2) * Rnd())
        CurrentY = CInt((38 - 2) * Rnd())

        While SolidMap(CurrentX, CurrentY) <> 0
            CurrentX = CInt((62 - 2) * Rnd())
            CurrentY = CInt((38 - 2) * Rnd())
        End While

        FloodMap(CurrentX, CurrentY) = 1

        MoveMade = True

        While MoveMade = True
            MoveMade = False
            For X = 0 To 63
                For Y = 0 To 39
                    Call SquareCompare(X, Y, MoveMade)
                Next
            Next
        End While

        'Call TestFloodFillRender()
    End Sub

#End Region

    Public Class CGFX

        Sub RenderMainMenu()

            Console.BackgroundColor = ConsoleColor.Black
            Console.Clear()

            Console.BackgroundColor = ConsoleColor.DarkGray
            Console.ForegroundColor = ConsoleColor.DarkGray

            For Y = 0 To 59
                Console.SetCursorPosition(0, Y)
                Console.Write("█")
                Console.SetCursorPosition(89, Y)
                Console.Write("█")
            Next

            For X = 0 To 89
                Console.SetCursorPosition(X, 0)
                Console.Write("█")
                Console.SetCursorPosition(X, 59)
                Console.Write("█")
            Next

            Console.ForegroundColor = ConsoleColor.Black

            For Y = 1 To 58
                Console.SetCursorPosition(1, Y)
                Console.Write("▒")
                Console.SetCursorPosition(88, Y)
                Console.Write("▒")
            Next

            For X = 1 To 88
                Console.SetCursorPosition(X, 1)
                Console.Write("▒")
                Console.SetCursorPosition(X, 58)
                Console.Write("▒")
            Next

            Console.ForegroundColor = ConsoleColor.Gray
            Console.BackgroundColor = ConsoleColor.Black
            Console.SetCursorPosition(4, 4)
            Console.Write("███ ███  ██ █ █ ██  ███ ███ ███ ███")
            Console.SetCursorPosition(4, 5)
            Console.Write(" █  █ █ █   █ █ █ █ █    █  █ █ █ █")
            Console.SetCursorPosition(4, 6)
            Console.Write(" ▒  ▒ ▒ ▒   ▒ ▒ ▒▒    ▒  ▒  ▒ ▒ ▒ ▒")
            'Console.Write(" █  █ █ █   █ █ ██    █  █  █ █ █ █")
            Console.SetCursorPosition(4, 7)
            Console.Write("▒▒▒ ▒ ▒  ▒▒ ▒▒▒ ▒ ▒ ▒▒▒ ▒▒▒ ▒▒▒ ▒ ▒")
            'Console.Write("███ █ █  ██ ███ █ █ ███ ███ ███ █ █")
            Console.ForegroundColor = ConsoleColor.DarkGray
            Console.SetCursorPosition(4, 9)
            Console.WriteLine("A tale of four kings")

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

        Sub RenderWorldMap()
            For y = 0 To WorldHeight
                For x = 0 To WorldWidth
                    Console.SetCursorPosition(x, y)
                    Console.BackgroundColor = TerrainLookupTable(1, WorldMap(x, y))
                    Console.ForegroundColor = TerrainLookupTable(0, WorldMap(x, y))
                    Console.Write(TerrainLookupTable(2, WorldMap(x, y)))
                Next
            Next
        End Sub

        Sub RenderSquare(ByVal PosX As Integer, ByVal PosY As Integer)
            Console.SetCursorPosition(PosX, PosY)
            Select Case VisibilityMap(PosX, PosY)
                Case 2
                    Console.ForegroundColor = TerrainLookupTable(0, TerrainMap(PosX, PosY))
                    Console.BackgroundColor = TerrainLookupTable(1, TerrainMap(PosX, PosY))
                    Console.Write(TerrainLookupTable(2, TerrainMap(PosX, PosY)))
                Case 1
                    Console.ForegroundColor = TerrainLookupTable(1, TerrainMap(PosX, PosY))
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write("▒")
                Case Else
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.Write(" ")
            End Select
            Console.SetCursorPosition(PosX, PosY)

            Console.BackgroundColor = ConsoleColor.Black
            Console.ForegroundColor = ConsoleColor.White
        End Sub

        Sub RenderGUI()
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
            Console.SetCursorPosition(0, 0)
            Console.Write("┌──────────────────────────────────────────────────────────────┬─────────────────────────┐")
            For Y = 1 To 39
                Console.SetCursorPosition(0, Y)
                Console.Write("│")
                Console.SetCursorPosition(89, Y)
                Console.Write("│")
                Console.SetCursorPosition(63, Y)
                Console.Write("│")
            Next
            Console.SetCursorPosition(0, 40)
            Console.Write("├──────────────────────────────────────────────────────────────┤                         │")
            For Y = 41 To 58
                Console.SetCursorPosition(0, Y)
                Console.Write("│")
                Console.SetCursorPosition(89, Y)
                Console.Write("│")
                Console.SetCursorPosition(63, Y)
                Console.Write("│")
            Next
            Console.SetCursorPosition(0, 59)
            Console.Write("└──────────────────────────────────────────────────────────────┴─────────────────────────┘")
            Console.SetCursorPosition(0, 0)

            Console.SetCursorPosition(64, 1)
            Console.Write(Player.Name)
            Console.SetCursorPosition(64, 2)
            Console.Write(Player.Role)
            Console.SetCursorPosition(78, 2)
            Console.Write("Floor: " & CurrentFloor)
            Console.SetCursorPosition(64, 3)
            Console.Write("Level: " & Player.Level & " Exp: " & Player.Experience(0) & " / " & Player.Experience(1))
            Console.SetCursorPosition(64, 5)
            Console.Write("Health:")
            Console.SetCursorPosition(68, 6)
            Console.Write(Player.Health & " / " & Player.MaxHealth)
            Console.SetCursorPosition(64, 7)
            Console.Write("Mana:")
            Console.SetCursorPosition(68, 8)
            Console.Write(Player.Mana & " / " & Player.MaxMana)
            Console.SetCursorPosition(64, 9)
            Console.Write("Hunger:")
            Console.SetCursorPosition(68, 10)
            Console.Write(Player.Hunger & " / 20")
            Console.SetCursorPosition(64, 12)
            Console.Write("Strength: " & Player.Strength)
            Console.SetCursorPosition(64, 13)
            Console.Write("Toughness: " & Player.Toughness)
            Console.SetCursorPosition(64, 14)
            Console.Write("Ranged: " & Player.RangedWeapons)
            Console.SetCursorPosition(64, 15)
            Console.Write("Melee: " & Player.MeleeWeapons)
            Console.SetCursorPosition(64, 16)
            Console.Write("Wisdom: " & Player.Wisdom)
            Console.SetCursorPosition(64, 17)
            Console.Write("Medicine: " & Player.Medicine)
            Console.SetCursorPosition(64, 18)
            Console.Write("Bartering: " & Player.Bartering)

            Console.SetCursorPosition(64, 20)
            Console.Write("Equipped Weapon:")
            Console.SetCursorPosition(68, 21)
            If Player.EquippedWeapon <> "N" Then
                Console.Write(WeaponTable(0, Player.EquippedWeapon) & " (+" & WeaponTable(2, Player.EquippedWeapon) & ")")
            Else
                Console.Write("Fists")
            End If
            Console.SetCursorPosition(64, 22)
            Console.Write("Shield:")
            Console.SetCursorPosition(68, 23)
            If Player.Equipment(4) <> "N" Then
                Console.Write(ArmourTable(0, Player.Equipment(4)) & "(+" & ArmourTable(2, Player.Equipment(4)) & ")")
            Else
                Console.Write("None")
            End If

            Console.SetCursorPosition(64, 25)
            Console.Write("Helmet:")
            Console.SetCursorPosition(68, 26)
            If Player.Equipment(0) <> "N" Then
                Console.Write(ArmourTable(0, Player.Equipment(0)) & "(+" & ArmourTable(2, Player.Equipment(0)) & ")")
            Else
                Console.Write("None")
            End If
            Console.SetCursorPosition(64, 27)
            Console.Write("Torso:")
            Console.SetCursorPosition(68, 28)
            If Player.Equipment(1) <> "N" Then
                Console.Write(ArmourTable(0, Player.Equipment(1)) & "(+" & ArmourTable(2, Player.Equipment(1)) & ")")
            Else
                Console.Write("None")
            End If
            Console.SetCursorPosition(64, 29)
            Console.Write("Arms:")
            Console.SetCursorPosition(68, 30)
            If Player.Equipment(3) <> "N" Then
                Console.Write(ArmourTable(0, Player.Equipment(3)) & "(+" & ArmourTable(2, Player.Equipment(3)) & ")")
            Else
                Console.Write("None")
            End If
            Console.SetCursorPosition(64, 31)
            Console.Write("Legs:")
            Console.SetCursorPosition(68, 32)
            If Player.Equipment(2) <> "N" Then
                Console.Write(ArmourTable(0, Player.Equipment(2)) & "(+" & ArmourTable(2, Player.Equipment(2)) & ")")
            Else
                Console.Write("None")
            End If

        End Sub

        Sub RenderOverworldGUI()
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
            Console.SetCursorPosition(0, 0)
            Console.Write("┌──────────────────────────────────────────────────────────────┬─────────────────────────┐")
            For Y = 1 To 39
                Console.SetCursorPosition(0, Y)
                Console.Write("│")
                Console.SetCursorPosition(89, Y)
                Console.Write("│")
                Console.SetCursorPosition(63, Y)
                Console.Write("│")
            Next
            Console.SetCursorPosition(0, 40)
            Console.Write("├──────────────────────────────────────────────────────────────┤                         │")
            For Y = 41 To 58
                Console.SetCursorPosition(0, Y)
                Console.Write("│")
                Console.SetCursorPosition(89, Y)
                Console.Write("│")
                Console.SetCursorPosition(63, Y)
                Console.Write("│")
            Next
            Console.SetCursorPosition(0, 59)
            Console.Write("└──────────────────────────────────────────────────────────────┴─────────────────────────┘")
            Console.SetCursorPosition(0, 0)

            Console.SetCursorPosition(64, 1)
            Console.Write(Player.Name)
            Console.SetCursorPosition(64, 2)
            Console.Write(Player.Role)
            Console.SetCursorPosition(64, 3)
            Console.Write("Level: " & Player.Level & " Exp: " & Player.Experience(0) & " / " & Player.Experience(1))
            Console.SetCursorPosition(64, 5)
            Console.Write("Health:")
            Console.SetCursorPosition(68, 6)
            Console.Write(Player.Health & " / " & Player.MaxHealth)
            Console.SetCursorPosition(64, 7)
            Console.Write("Mana:")
            Console.SetCursorPosition(68, 8)
            Console.Write(Player.Mana & " / " & Player.MaxMana)
            Console.SetCursorPosition(64, 9)
            Console.Write("Hunger:")
            Console.SetCursorPosition(68, 10)
            Console.Write(Player.Hunger & " / 20")
            Console.SetCursorPosition(64, 12)
            Console.Write("Strength: " & Player.Strength)
            Console.SetCursorPosition(64, 13)
            Console.Write("Toughness: " & Player.Toughness)
            Console.SetCursorPosition(64, 14)
            Console.Write("Ranged: " & Player.RangedWeapons)
            Console.SetCursorPosition(64, 15)
            Console.Write("Melee: " & Player.MeleeWeapons)
            Console.SetCursorPosition(64, 16)
            Console.Write("Wisdom: " & Player.Wisdom)
            Console.SetCursorPosition(64, 17)
            Console.Write("Medicine: " & Player.Medicine)
            Console.SetCursorPosition(64, 18)
            Console.Write("Bartering: " & Player.Bartering)

            Console.SetCursorPosition(64, 20)
            Console.Write("Equipped Weapon:")
            Console.SetCursorPosition(68, 21)
            If Player.EquippedWeapon <> "N" Then
                Console.Write(WeaponTable(0, Player.EquippedWeapon) & " (+" & WeaponTable(2, Player.EquippedWeapon) & ")")
            Else
                Console.Write("Fists")
            End If

            Console.SetCursorPosition(64, 22)
            Console.Write("Shield:")
            Console.SetCursorPosition(68, 23)
            If Player.Equipment(4) <> "N" Then
                Console.Write(ArmourTable(0, Player.Equipment(4)) & "(+" & ArmourTable(2, Player.Equipment(4)) & ")")
            Else
                Console.Write("None")
            End If

            Console.SetCursorPosition(64, 25)
            Console.Write("Helmet:")
            Console.SetCursorPosition(68, 26)
            If Player.Equipment(0) <> "N" Then
                Console.Write(ArmourTable(0, Player.Equipment(0)) & "(+" & ArmourTable(2, Player.Equipment(0)) & ")")
            Else
                Console.Write("None")
            End If
            Console.SetCursorPosition(64, 27)
            Console.Write("Torso:")
            Console.SetCursorPosition(68, 28)
            If Player.Equipment(1) <> "N" Then
                Console.Write(ArmourTable(0, Player.Equipment(1)) & "(+" & ArmourTable(2, Player.Equipment(1)) & ")")
            Else
                Console.Write("None")
            End If
            Console.SetCursorPosition(64, 29)
            Console.Write("Arms:")
            Console.SetCursorPosition(68, 30)
            If Player.Equipment(3) <> "N" Then
                Console.Write(ArmourTable(0, Player.Equipment(3)) & "(+" & ArmourTable(2, Player.Equipment(3)) & ")")
            Else
                Console.Write("None")
            End If
            Console.SetCursorPosition(64, 31)
            Console.Write("Legs:")
            Console.SetCursorPosition(68, 32)
            If Player.Equipment(2) <> "N" Then
                Console.Write(ArmourTable(0, Player.Equipment(2)) & "(+" & ArmourTable(2, Player.Equipment(2)) & ")")
            Else
                Console.Write("None")
            End If

        End Sub

        Sub RenderEnemies()

            For X = 0 To EnemyNumber
                If Enemy(X).Enabled = True Then
                    If VisibilityMap(Enemy(X).PosX, Enemy(X).PosY) = 2 Then
                        Console.BackgroundColor = Enemy(X).BackColour
                        Console.ForegroundColor = Enemy(X).ForeColour
                        Console.SetCursorPosition(Enemy(X).PosX, Enemy(X).PosY)
                        Console.Write(Enemy(X).Pic)
                    End If
                End If
            Next

        End Sub

        Sub RenderEventLog()
            Dim Count As Integer
            Dim StringToWrite As String
            Dim CharArray() As Char
            Dim X As Integer
            Dim Y As Integer
            Console.ForegroundColor = 0
            For Count = 41 To 58
                Console.SetCursorPosition(1, Count)
                Console.Write("                                                              ")
            Next
            If EventlogSize >= 0 Then
                Console.ForegroundColor = ConsoleColor.White
                Console.BackgroundColor = ConsoleColor.Black

                X = 1
                Y = 41

                For Count = EventlogSize To 0 Step -1
                    StringToWrite = EventLog(Count)
                    CharArray = StringToWrite.ToCharArray()
                    Console.SetCursorPosition(X, Y)
                    If Y > 44 Then
                        Console.ForegroundColor = ConsoleColor.DarkGray
                    End If
                    For C = 0 To StringToWrite.Length - 1
                        If X <> 63 Then
                            Console.Write(CharArray(C))
                        Else
                            If Y <> 58 Then
                                X = 1
                                Y += 1
                                Console.SetCursorPosition(X, Y)
                            Else
                                Return
                            End If
                        End If
                    Next
                    Console.ForegroundColor = ConsoleColor.Gray
                    If Y < 58 Then
                        Y += 1
                        X = 1
                    End If
                Next
            End If
        End Sub

        Sub SlowWrite(ByVal X As Integer, ByVal Y As Integer, ByVal Speed As Integer, ByVal StringToWrite As String)
            Dim StringArray() As Char
            Dim StringLength As Integer
            Dim OutputString As String

            Console.SetCursorPosition(X, Y)
            StringLength = StringToWrite.Length
            ReDim StringArray(StringLength)
            StringArray = StringToWrite.ToCharArray

            Console.ForegroundColor = ConsoleColor.DarkGray

            'For C = 0 To StringLength - 1
            '    If StringArray(C) <> " " Then
            '        OutputString += "░"
            '    Else
            '        OutputString += " "
            '    End If
            'Next

            Console.Write(StringToWrite)
            System.Threading.Thread.Sleep(Speed)
            Console.ForegroundColor = ConsoleColor.Gray
            Console.SetCursorPosition(X, Y)
            Console.Write(StringToWrite)
            System.Threading.Thread.Sleep(Speed)
            Console.ForegroundColor = ConsoleColor.White
            Console.SetCursorPosition(X, Y)
            Console.Write(StringToWrite)

            'For C = 0 To StringLength - 1
            'Console.Write(StringArray(C))
            'System.Threading.Thread.Sleep(Speed)
            'Next

        End Sub

    End Class

    Public Class Projectile
        Public DestX As Integer
        Public DestY As Integer
        Public X As Double
        Public Y As Double
        Public Type As Integer

        Sub Fire()
            Dim Pic As String
            Dim Gradient As Double
            Dim Xdirection As Integer
            Dim OldX As Integer
            Dim OldY As Integer
            Select Case Type
                Case 0  '9mm
                    Pic = "·"
                Case 1  '5.56mm
                    Pic = "·"
                Case 2  'Shotgun Shell
                    Pic = "░"
                Case 3  'Grenade
                    Pic = "○"
                Case 4  'Energy Cell
                    Pic = "•"
                Case 5
                    Pic = "·"   'Enemy Shot
            End Select

            If Y <> DestY Then
                If X = DestX Then
                    If Y < DestY Then
                        Gradient = 1
                    Else
                        Gradient = -1
                    End If
                Else
                    Gradient = (DestY - Y) / Math.Abs(DestX - X)
                End If
            Else
                Gradient = 0
            End If

            If X > DestX Then
                Xdirection = -1
            ElseIf X < DestX Then
                Xdirection = 1
            Else
                Xdirection = 0
            End If

            OldX = X
            OldY = Y
            Console.SetCursorPosition(X, Y)

            While SolidMap(X, CInt(Y)) <> 1
                OldX = X
                OldY = CInt(Y)
                Console.SetCursorPosition(OldX, OldY)
                ConsoleOutput.RenderSquare(OldX, OldY)
                X += Xdirection
                Y += Gradient
                Console.SetCursorPosition(X, Y)
                If SolidMap(X, CInt(Y)) = 0 Then
                    If Type <> 5 Then
                        If Entitymap(X, CInt(Y)) = 0 Then
                            If VisibilityMap(X, CInt(Y)) = 2 Then
                                Console.BackgroundColor = TerrainLookupTable(1, TerrainMap(X, CInt(Y)))
                                Select Case Type
                                    Case 1
                                        Console.ForegroundColor = ConsoleColor.Black
                                    Case 2
                                        Console.ForegroundColor = ConsoleColor.Black
                                    Case 3
                                        Console.ForegroundColor = ConsoleColor.Black
                                    Case 4
                                        Console.ForegroundColor = ConsoleColor.DarkGreen
                                    Case 5
                                        Console.ForegroundColor = ConsoleColor.DarkCyan
                                End Select
                                Console.Write(Pic)
                            End If
                        Else
                            Enemy(Entitymap(X, CInt(Y)) - 2).Angered = True
                            Enemy(Entitymap(X, CInt(Y)) - 2).Shot()
                            Exit Sub
                        End If
                    Else
                        If X = Player.PosX Then
                            If Y = Player.PosY Then
                                Exit Sub
                            End If
                        End If
                    End If
                Else
                    If VisibilityMap(X, CInt(Y)) = 2 Then
                        Console.BackgroundColor = TerrainLookupTable(1, TerrainMap(X, CInt(Y)))
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write("☼")
                        System.Threading.Thread.Sleep(200)
                        Console.ForegroundColor = ConsoleColor.White
                        Console.BackgroundColor = ConsoleColor.Black
                        EventlogSize += 1
                        ReDim Preserve EventLog(EventlogSize)
                        If VisibilityMap(X, Y) <> 2 Then
                            EventLog(EventlogSize) = "You hear the shot impact a wall in the distance."
                        Else
                            EventLog(EventlogSize) = "The shot hits a wall."
                        End If
                        ConsoleOutput.RenderEventLog()
                        ConsoleOutput.RenderSquare(X, CInt(Y))
                        Exit Sub
                    End If
                End If
                System.Threading.Thread.Sleep(50)
            End While
        End Sub

    End Class

    Public Class Entity
        Public Enabled As Boolean
        Public Pic As String
        Public Name As String
        Public BackColour As Integer
        Public ForeColour As Integer

        Public Level As Integer
        Public Gold As Integer      'Gold dropped on kill
        Public Exp As Integer       'Experience dropped on kill

        Public Health As Integer
        Public Attack As Integer
        Public Defence As Integer
        Public Agility As Integer
        Public Accuracy As Integer

        Public Aggressive As Boolean        'Will the entity attack the player on sight?
        Public Angered As Boolean           'Is the entity aggroed?
        Public AIType As Integer            '0 = chase player and melee, 1 = stay at range and fire projectiles
        Public EntityMapID As Integer

        Public PosX As Integer
        Public PosY As Integer

        Private MovementPath(,) As Integer
        Private CurrentMovementTrackID As Integer
        Private PathDistance As Integer
        Public DistanceToPlayer As Integer

        Sub Initialise(ByVal EnemyType)
            Dim X As Integer
            Dim Y As Integer

            Select Case EnemyType

                Case 0
                    Me.Name = "Rat"
                    Me.Pic = "r"
                    Me.Health = 4
                    Me.Attack = 2
                    Me.Defence = 1
                    Me.Agility = 3
                    Me.Accuracy = 0
                    Me.ForeColour = 12
                    Me.Gold = 5
                    Me.Exp = 5
                    Me.AIType = 0
                    Me.Aggressive = False
                Case 1
                    Me.Name = "Menacing Rat"
                    Me.Pic = "r"
                    Me.Health = 6
                    Me.Attack = 3
                    Me.Defence = 2
                    Me.Agility = 3
                    Me.Accuracy = 0
                    Me.ForeColour = 4
                    Me.Gold = 10
                    Me.Exp = 10
                    Me.AIType = 0
                    Me.Aggressive = False
                Case 2
                    Me.Name = "Large Rat"
                    Me.Pic = "R"
                    Me.Health = 8
                    Me.Attack = 6
                    Me.Defence = 4
                    Me.Agility = 3
                    Me.Accuracy = 0
                    Me.ForeColour = 4
                    Me.Gold = 20
                    Me.Exp = 20
                    Me.AIType = 0
                    Me.Aggressive = False
                Case 3
                    Me.Name = "Land Squid"
                    Me.Pic = "s"
                    Me.Health = 12
                    Me.Attack = 3
                    Me.Defence = 3
                    Me.Agility = 3
                    Me.Accuracy = 4
                    Me.ForeColour = 11
                    Me.Gold = 40
                    Me.Exp = 40
                    Me.AIType = 1
                    Me.Aggressive = False
                Case 4
                    Me.Name = "Menacing Land Squid"
                    Me.Pic = "s"
                    Me.Health = 14
                    Me.Attack = 5
                    Me.Defence = 5
                    Me.Agility = 3
                    Me.Accuracy = 4
                    Me.ForeColour = 3
                    Me.Gold = 80
                    Me.Exp = 80
                    Me.AIType = 1
                    Me.Aggressive = True
                Case 5
                    Me.Name = "large Land Squid"
                    Me.Pic = "S"
                    Me.Health = 18
                    Me.Attack = 6
                    Me.Defence = 8
                    Me.Agility = 3
                    Me.Accuracy = 6
                    Me.ForeColour = 3
                    Me.Gold = 160
                    Me.Exp = 160
                    Me.AIType = 1
                    Me.Aggressive = True
                Case 6
                    Me.Name = "Gnome"
                    Me.Pic = "g"
                    Me.Health = 20
                    Me.Attack = 11
                    Me.Defence = 11
                    Me.Agility = 3
                    Me.Accuracy = 0
                    Me.ForeColour = 10
                    Me.Gold = 320
                    Me.Exp = 320
                    Me.AIType = 0
                    Me.Aggressive = True
                Case 7
                    Me.Name = "Menacing Gnome"
                    Me.Pic = "g"
                    Me.Health = 24
                    Me.Attack = 13
                    Me.Defence = 13
                    Me.Agility = 3
                    Me.Accuracy = 0
                    Me.ForeColour = 2
                    Me.Gold = 640
                    Me.Exp = 640
                    Me.AIType = 0
                    Me.Aggressive = True
                Case 8
                    Me.Name = "Large Gnome"
                    Me.Pic = "G"
                    Me.Health = 30
                    Me.Attack = 15
                    Me.Defence = 15
                    Me.Agility = 3
                    Me.Accuracy = 0
                    Me.ForeColour = 2
                    Me.Gold = 1280
                    Me.Exp = 1280
                    Me.AIType = 0
                    Me.Aggressive = True
                Case 9
                    Me.Name = "Purple Moose"
                    Me.Pic = "m"
                    Me.Health = 40
                    Me.Attack = 18
                    Me.Defence = 16
                    Me.Agility = 3
                    Me.Accuracy = 0
                    Me.ForeColour = 13
                    Me.Gold = 2560
                    Me.Exp = 2560
                    Me.AIType = 0
                    Me.Aggressive = False
                Case 10
                    Me.Name = "Mildly Menacing Purple Moose"
                    Me.Pic = "m"
                    Me.Health = 50
                    Me.Attack = 20
                    Me.Defence = 18
                    Me.Agility = 3
                    Me.Accuracy = 0
                    Me.ForeColour = 5
                    Me.Gold = 5120
                    Me.Exp = 5120
                    Me.AIType = 0
                    Me.Aggressive = False
                Case 11
                    Me.Name = "Large Purple Moose"
                    Me.Pic = "M"
                    Me.Health = 60
                    Me.Attack = 22
                    Me.Defence = 20
                    Me.Agility = 3
                    Me.Accuracy = 0
                    Me.ForeColour = 5
                    Me.Gold = 10240
                    Me.Exp = 10240
                    Me.AIType = 0
                    Me.Aggressive = True

                Case 12
                    Me.Name = "Red Dragon"
                    Me.Pic = "D"
                    Me.Health = 80
                    Me.Attack = 25
                    Me.Defence = 25
                    Me.Agility = 3
                    Me.Accuracy = 0
                    Me.ForeColour = 12
                    Me.Gold = 20480
                    Me.Exp = 20480
                    Me.AIType = 0
                    Me.Aggressive = True

            End Select

            X = CInt((62 - 2) * Rnd())
            Y = CInt((38 - 2) * Rnd())
            While SolidMap(X, Y) = 1

                X = CInt((62 - 2) * Rnd())
                Y = CInt((38 - 2) * Rnd())
            End While
            Me.PosX = X
            Me.PosY = Y
            X = CInt((62 - 2) * Rnd())
            Y = CInt((38 - 2) * Rnd())
            While SolidMap(X, Y) = 1
                X = CInt((62 - 2) * Rnd())
                Y = CInt((38 - 2) * Rnd())
            End While
            Entitymap(PosX, PosY) = Me.EntityMapID
            Me.BackColour = TerrainLookupTable(1, TerrainMap(PosX, PosY))
            Me.Pathfind(X, Y)

        End Sub

        Sub Pathfind(ByVal DestX, ByVal DestY)
            Dim OpenList(,) As Integer
            Dim ClosedList(,) As Integer
            Dim VisitedSquares(63, 39) As Integer
            Dim F As Integer
            Dim G As Integer
            Dim H As Integer
            Dim OpenListEntryNumber As Integer
            Dim ClosedListEntryNumber As Integer
            Dim ParentID As Integer
            Dim DesiredID As Integer
            Dim CanMove As Boolean = False

            Dim CurrentX As Integer
            Dim CurrentY As Integer

            CurrentX = PosX
            CurrentY = PosY

            PathDistance = 0

            For X = 0 To 63
                For Y = 0 To 39
                    VisitedSquares(X, Y) = 0
                Next
            Next

            VisitedSquares(CurrentX, CurrentY) = 1

            ReDim Preserve ClosedList(1, 0)
            ClosedList(0, 0) = CurrentX
            ClosedList(1, 0) = CurrentY

            OpenListEntryNumber = -1
            ClosedListEntryNumber = 0
            G = 10

            While ((CurrentX <> DestX) Or (CurrentY <> DestY))

                CanMove = False

                If SolidMap(CurrentX + 1, CurrentY) <> 1 Then
                    If VisitedSquares(CurrentX + 1, CurrentY) = 0 Then
                        OpenListEntryNumber += 1
                        ReDim Preserve OpenList(3, OpenListEntryNumber)
                        OpenList(0, OpenListEntryNumber) = CurrentX + 1
                        OpenList(1, OpenListEntryNumber) = CurrentY
                        OpenList(2, OpenListEntryNumber) = ParentID
                        H = Math.Abs((CurrentX + 1) - DestX) + Math.Abs(CurrentY - DestY)
                        F = G + H
                        OpenList(3, OpenListEntryNumber) = F
                        CanMove = True
                    End If
                End If

                If SolidMap(CurrentX - 1, CurrentY) <> 1 Then
                    If VisitedSquares(CurrentX - 1, CurrentY) = 0 Then
                        OpenListEntryNumber += 1
                        ReDim Preserve OpenList(3, OpenListEntryNumber)
                        OpenList(0, OpenListEntryNumber) = CurrentX - 1
                        OpenList(1, OpenListEntryNumber) = CurrentY
                        OpenList(2, OpenListEntryNumber) = ParentID
                        H = Math.Abs((CurrentX - 1) - DestX) + Math.Abs(CurrentY - DestY)
                        F = G + H
                        OpenList(3, OpenListEntryNumber) = F
                        CanMove = True
                    End If
                End If

                If SolidMap(CurrentX, CurrentY + 1) <> 1 Then
                    If VisitedSquares(CurrentX, CurrentY + 1) = 0 Then
                        OpenListEntryNumber += 1
                        ReDim Preserve OpenList(3, OpenListEntryNumber)
                        OpenList(0, OpenListEntryNumber) = CurrentX
                        OpenList(1, OpenListEntryNumber) = CurrentY + 1
                        OpenList(2, OpenListEntryNumber) = ParentID
                        H = Math.Abs(CurrentX - DestX) + Math.Abs((CurrentY + 1) - DestY)
                        F = G + H
                        OpenList(3, OpenListEntryNumber) = F
                        CanMove = True
                    End If
                End If

                If SolidMap(CurrentX, CurrentY - 1) <> 1 Then
                    If VisitedSquares(CurrentX, CurrentY - 1) = 0 Then
                        OpenListEntryNumber += 1
                        ReDim Preserve OpenList(3, OpenListEntryNumber)
                        OpenList(0, OpenListEntryNumber) = CurrentX
                        OpenList(1, OpenListEntryNumber) = CurrentY - 1
                        OpenList(2, OpenListEntryNumber) = ParentID
                        H = Math.Abs(CurrentX - DestX) + Math.Abs((CurrentY - 1) - DestY)
                        F = G + H
                        OpenList(3, OpenListEntryNumber) = F
                        CanMove = True
                    End If
                End If

                If CanMove = False Then
                    Exit While
                End If
                DesiredID = 0

                For Count = 0 To OpenListEntryNumber
                    If OpenList(3, Count) < OpenList(3, DesiredID) Then
                        DesiredID = Count
                    End If
                Next

                ClosedListEntryNumber += 1

                ReDim Preserve ClosedList(1, ClosedListEntryNumber)

                ClosedList(0, ClosedListEntryNumber) = OpenList(0, DesiredID)
                ClosedList(1, ClosedListEntryNumber) = OpenList(1, DesiredID)

                CurrentX = OpenList(0, DesiredID)
                CurrentY = OpenList(1, DesiredID)

                VisitedSquares(CurrentX, CurrentY) = 1

                OpenListEntryNumber = -1
            End While

            ReDim Preserve MovementPath(1, ClosedListEntryNumber)

            For C = 0 To ClosedListEntryNumber
                MovementPath(0, C) = ClosedList(0, C)
                MovementPath(1, C) = ClosedList(1, C)
                PathDistance += 1
            Next

            CurrentMovementTrackID = 0

            DistanceToPlayer = PathDistance

        End Sub

        Sub Move()
            Dim X As Integer
            Dim Y As Integer
            If Me.Aggressive = True Then
                If VisibilityMap(PosX, PosY) = 2 Then
                    Me.Angered = True
                End If
            End If
            If Me.Angered = False Then
                Try
                    CurrentMovementTrackID += 1
                    X = MovementPath(0, CurrentMovementTrackID)
                    Y = MovementPath(1, CurrentMovementTrackID)

                    If Entitymap(X, Y) = 0 Then
                        Me.BackColour = TerrainLookupTable(1, TerrainMap(PosX, PosY))
                        Entitymap(PosX, PosY) = 0
                        Entitymap(X, Y) = Me.EntityMapID
                        PosX = X
                        PosY = Y
                    End If
                Catch ex As Exception

                    X = CInt((62 - 2) * Rnd())
                    Y = CInt((38 - 2) * Rnd())
                    While SolidMap(X, Y) = 1
                        X = CInt((62 - 2) * Rnd())
                        Y = CInt((38 - 2) * Rnd())
                    End While

                    Me.Pathfind(X, Y)

                End Try
            Else
                If VisibilityMap(Me.PosX, Me.PosY) = 2 Then
                    Me.Pathfind(Player.PosX, Player.PosY)
                    If AIType = 0 Then
                        CurrentMovementTrackID += 1
                        X = MovementPath(0, CurrentMovementTrackID)
                        Y = MovementPath(1, CurrentMovementTrackID)
                    Else
                        If DistanceToPlayer > 6 Then
                            CurrentMovementTrackID += 1
                            X = MovementPath(0, CurrentMovementTrackID)
                            Y = MovementPath(1, CurrentMovementTrackID)
                        Else
                            Bullet.X = PosX
                            Bullet.Y = PosY
                            Bullet.DestX = Player.PosX
                            Bullet.DestY = Player.PosY
                            Bullet.Type = 5
                            Bullet.Fire()
                            Player.Shot(Me.Name, Me.Accuracy, Me.Attack)
                            Me.Pathfind(Player.PosX, Player.PosY)
                            Exit Sub
                        End If
                    End If

                    If Entitymap(X, Y) = 0 Then
                        Me.BackColour = TerrainLookupTable(1, TerrainMap(PosX, PosY))
                        Entitymap(PosX, PosY) = 0
                        Entitymap(X, Y) = Me.EntityMapID
                        PosX = X
                        PosY = Y
                    ElseIf Entitymap(X, Y) = 1 Then
                        Player.Attacked(Me.Attack, Me.Name)
                    End If
                Else
                    Try
                        CurrentMovementTrackID += 1
                        X = MovementPath(0, CurrentMovementTrackID)
                        Y = MovementPath(1, CurrentMovementTrackID)

                        If Entitymap(X, Y) = 0 Then
                            Me.BackColour = TerrainLookupTable(1, TerrainMap(PosX, PosY))
                            Entitymap(PosX, PosY) = 0
                            Entitymap(X, Y) = Me.EntityMapID
                            PosX = X
                            PosY = Y
                        End If
                    Catch ex As Exception

                        X = CInt((62 - 2) * Rnd())
                        Y = CInt((38 - 2) * Rnd())
                        While SolidMap(X, Y) = 1
                            X = CInt((62 - 2) * Rnd())
                            Y = CInt((38 - 2) * Rnd())
                        End While

                        Angered = False
                        Me.Pathfind(X, Y)

                    End Try
                End If
            End If
        End Sub

        Sub Attacked()
            Dim Dice As Integer
            Dim Damage As Integer
            Dim Armour As Integer

            Dice = CInt((Player.MeleeWeapons * Rnd()) + 1)

            Damage = Dice + CInt(Player.Strength / 3)

            If Player.EquippedWeapon <> "N" Then
                Damage += WeaponTable(2, Player.EquippedWeapon)
            End If

            Dice = CInt((3 * Rnd()) + 1)

            Armour = Dice + Me.Defence

            If Damage > 0 Then
                Me.Health -= Damage
                EventlogSize += 1
                ReDim Preserve EventLog(EventlogSize)
                EventLog(EventlogSize) = "You hit the " & Me.Name & " for " & Damage & " damage!"
                ConsoleOutput.RenderEventLog()
            Else
                EventlogSize += 1
                ReDim Preserve EventLog(EventlogSize)
                EventLog(EventlogSize) = "your attack glances off the " & Me.Name & "!"
                ConsoleOutput.RenderEventLog()
            End If

            If Me.Health <= 0 Then
                Me.Kill()
            End If

        End Sub

        Sub Shot()
            Dim Dice As Integer
            Dim Damage As Integer
            Dim Armour As Integer

            Dice = (((11 + Me.Agility) * Rnd()) + 1)

            If Dice > Player.RangedWeapons Then
                EventlogSize += 1
                ReDim Preserve EventLog(EventlogSize)
                EventLog(EventlogSize) = "Your shot missed the " & Me.Name & "!"
                ConsoleOutput.RenderEventLog()
            Else
                Damage = CInt(Player.RangedWeapons / 3) + WeaponTable(2, Player.EquippedWeapon)
                Armour = Me.Defence + (2 * Rnd() - 1)
                If Damage > Armour Then
                    Me.Health -= Damage
                    EventlogSize += 1
                    ReDim Preserve EventLog(EventlogSize)
                    EventLog(EventlogSize) = "Your shot hits the " & Me.Name & " for " & Damage & " damage!"
                    ConsoleOutput.RenderEventLog()
                Else
                    EventlogSize += 1
                    ReDim Preserve EventLog(EventlogSize)
                    EventLog(EventlogSize) = "Your shot glances off the " & Me.Name & "'s armour! "
                    ConsoleOutput.RenderEventLog()
                End If
            End If

            If Me.Health <= 0 Then
                Me.Kill()
            End If
        End Sub

        Sub Kill()
            Dim GoldToAdd As Integer
            GoldToAdd = CInt((Me.Gold * Rnd()) + 1)
            Me.Enabled = False
            Entitymap(PosX, PosY) = 0
            EventlogSize += 1
            ReDim Preserve EventLog(EventlogSize)
            EventLog(EventlogSize) = "You kill the " & Me.Name & "!"
            EventlogSize += 1
            ReDim Preserve EventLog(EventlogSize)
            EventLog(EventlogSize) = "The " & Me.Name & " dropped " & GoldToAdd & " gold and " & Me.Exp & " experience!"
            ConsoleOutput.RenderEventLog()
            Player.KillCount += 1
            Player.Gold += GoldToAdd
            Player.Experience(0) += Me.Exp
            Entitymap(PosX, PosY) = 0
            Call Player.CheckLevel()
            ConsoleOutput.RenderGUI()
        End Sub

    End Class

    Public Class UserControlledCharacter
        Public PosX As Integer
        Public PosY As Integer

        Public Fort1Cleared As Boolean
        Public Fort2Cleared As Boolean
        Public Fort3Cleared As Boolean
        Public Fort4Cleared As Boolean

        Public Name As String           'The player's name
        Public Gender As Integer        'The players gender. 0 = Male, 1 = Female
        Public Role As String           'The player's class
        Public Health As Integer        'The health points of the player. If these fall to 0, the player dies.
        Public Hunger As Integer        'The player's level of hunger. If this falls below 0, the player dies.
        Public Mana As Integer          'The player's current level of magical energy.
        Public Armour As Integer        'The player's current total armour value.
        Public EquipmentLoad As Double  'The player's current weight of all equipped items.
        Public MaxEquipmentLoad As Double   'The maximum weight the player can ahve equipped.
        Public CarryWeight As Double    'The maximum weight the player can carry in their backpack.
        Public TimeAlive As Integer     'The in-game time the player has survived for in turns. (1 turn = 5 seconds)
        Public KillCount As Integer     'The number of enemies killed during the player's lifetime

        Public Level As Integer         'The player's level
        Public Experience(1) As Integer 'The current number of experience points the player posesses followed by the number required to level up.
        Public MaxHealth As Integer     'The maximum value of the player's health.
        Public MaxMana As Integer       'The maximum value of the player's mana.
        Public Strength As Integer      'The physical strength of the player. Used when determining physical attacks such as punching or using a melee weapon and determining what armour and equipment the player can equip.
        Public Toughness As Integer     'The physical defence of the player. Used as the 'base' defence upon which armour values are stacked.
        Public RangedWeapons As Integer 'The player's proficiency with ranged weapons.
        Public MeleeWeapons As Integer  'The player's proficiency with melee weapons.
        Public Wisdom As Integer        'The player's proficiency with magic and magical artefacts.
        Public Medicine As Integer      'The player's knowledge of and proficiency with medicinal herbs and healing spells.
        Public Bartering As Integer     'The player's skill with money. Higher skill means lower prices.

        Public Equipment(4) As String     'The armour/items equipped on the player in the following order: Head, Torso, Legs, Arms, Shield. The item ID indicates what is equipped.
        Public Inventory(2, 0) As String    'Item Type, ItemID, Quantity.
        Public InventorySize As Integer
        Public EquippedWeapon As String    'Item ID
        Public Ranged As Boolean            'True means a ranged weapon is equipped. False means a melee weapon is equipped.
        Public Arrows As Integer            'Stores the player's arrows
        Public Gold As Integer              'Stores the player's gold pieces
        Public NewlyEnteredDungeon As Boolean

        Public LastTimeHungerUpdated   'The last value of timealive when the hunger was checked.

        Sub Initialise()
            CalculateNewArmourValue()
        End Sub

        Sub CalculateNewArmourValue()

            Armour = 0

            For C = 0 To 4
                If Equipment(C) <> Nothing Then
                    Armour += ArmourTable(1, Equipment(C))
                End If
            Next

        End Sub

        Sub Move()
            Dim RenderFromSaveMap As Boolean = True
            Dim CursorX As Integer
            Dim CursorY As Integer
            Dim Key As Integer
            Dim OldPosX As Integer
            Dim OldPosY As Integer
            Dim InteractChoiceMade As Boolean = False
            Dim KeyHandled As Boolean

            OldPosX = PosX
            OldPosY = PosY

            KeyHandled = False

            While KeyHandled = False

                Key = Console.ReadKey(True).Key
                Select Case Key
                    Case 38                                                     'Up arrow
                        Try
                            If TerrainMap(PosX, PosY) = 3 Then
                                SolidMap(PosX, PosY) = 2
                            End If
                            If SolidMap(PosX, PosY - 1) <> 1 Then
                                If Entitymap(PosX, PosY - 1) = 0 Then
                                    PosY = PosY - 1
                                    If SolidMap(PosX, PosY) = 2 Then
                                        SolidMap(PosX, PosY) = 0
                                    End If
                                    Console.SetCursorPosition(PosX, PosY - 1)
                                    KeyHandled = True
                                Else
                                    If Me.Ranged = False Then
                                        Enemy(Entitymap(PosX, PosY - 1) - 2).Angered = True
                                        Enemy(Entitymap(PosX, PosY - 1) - 2).Attacked()
                                        KeyHandled = True
                                    End If
                                End If
                            End If
                        Catch ex As Exception
                        End Try

                    Case 40                                                     'Down arrow
                        Try
                            If TerrainMap(PosX, PosY) = 3 Then
                                SolidMap(PosX, PosY) = 2
                            End If
                            If SolidMap(PosX, PosY + 1) <> 1 Then
                                If Entitymap(PosX, PosY + 1) = 0 Then
                                    PosY = PosY + 1
                                    If SolidMap(PosX, PosY) = 2 Then
                                        SolidMap(PosX, PosY) = 0
                                    End If
                                    Console.SetCursorPosition(PosX, PosY + 1)
                                    KeyHandled = True
                                Else
                                    If Me.Ranged = False Then
                                        Enemy(Entitymap(PosX, PosY + 1) - 2).Angered = True
                                        Enemy(Entitymap(PosX, PosY + 1) - 2).Attacked()
                                        KeyHandled = True
                                    End If
                                End If
                            End If
                        Catch ex As Exception
                        End Try
                    Case 37                                                     'Left arrow
                        Try
                            If TerrainMap(PosX, PosY) = 3 Then
                                SolidMap(PosX, PosY) = 2
                            End If
                            If SolidMap(PosX - 1, PosY) <> 1 Then
                                If Entitymap(PosX - 1, PosY) = 0 Then
                                    PosX = PosX - 1
                                    If SolidMap(PosX, PosY) = 2 Then
                                        SolidMap(PosX, PosY) = 0
                                    End If
                                    Console.SetCursorPosition(PosX - 1, PosY)
                                    KeyHandled = True
                                Else
                                    If Me.Ranged = False Then
                                        Enemy(Entitymap(PosX - 1, PosY) - 2).Angered = True
                                        Enemy(Entitymap(PosX - 1, PosY) - 2).Attacked()
                                        KeyHandled = True
                                    End If
                                End If
                            End If
                        Catch ex As Exception
                        End Try
                    Case 39                                                     'Right arrow
                        Try
                            If TerrainMap(PosX, PosY) = 3 Then
                                SolidMap(PosX, PosY) = 2
                            End If
                            If SolidMap(PosX + 1, PosY) <> 1 Then
                                If Entitymap(PosX + 1, PosY) = 0 Then
                                    PosX = PosX + 1
                                    If SolidMap(PosX, PosY) = 2 Then
                                        SolidMap(PosX, PosY) = 0
                                    End If
                                    Console.SetCursorPosition(PosX + 1, PosY)
                                    KeyHandled = True
                                Else
                                    If Me.Ranged = False Then
                                        Enemy(Entitymap(PosX + 1, PosY) - 2).Angered = True
                                        Enemy(Entitymap(PosX + 1, PosY) - 2).Attacked()
                                        KeyHandled = True
                                    End If
                                End If
                            End If
                        Catch ex As Exception
                        End Try
                    Case 75                                                    'K key
                        InteractChoiceMade = False
                        CursorX = PosX
                        CursorY = PosY
                        While InteractChoiceMade = False
                            Console.SetCursorPosition(PosX, PosY)
                            Console.BackgroundColor = TerrainLookupTable(1, TerrainMap(PosX, PosY))
                            'Select Case TerrainMap(PosX, PosY)
                            '    Case 0
                            'Console.BackgroundColor = ConsoleColor.Black
                            '    Case 1
                            'Console.BackgroundColor = ConsoleColor.Gray
                            '    Case 2
                            'Console.BackgroundColor = ConsoleColor.DarkGray
                            '    Case 3
                            'Console.BackgroundColor = ConsoleColor.DarkYellow
                            '    Case 10
                            'Console.BackgroundColor = ConsoleColor.Cyan
                            '    Case 11
                            'Console.BackgroundColor = ConsoleColor.DarkRed
                            'End Select
                            Console.Write("☺")
                            ConsoleOutput.RenderEnemies()
                            Console.SetCursorPosition(CursorX, CursorY)
                            Console.BackgroundColor = TerrainLookupTable(1, TerrainMap(CursorX, CursorY))
                            'Select Case TerrainMap(CursorX, CursorY)
                            '    Case 0
                            'Console.BackgroundColor = ConsoleColor.Black
                            '    Case 1
                            'Console.BackgroundColor = ConsoleColor.Gray
                            '    Case 2
                            'Console.BackgroundColor = ConsoleColor.DarkGray
                            '    Case 3
                            'Console.BackgroundColor = ConsoleColor.DarkYellow
                            '    Case 10
                            'Console.BackgroundColor = ConsoleColor.Cyan
                            '    Case 11
                            'Console.BackgroundColor = ConsoleColor.DarkRed
                            'End Select
                            Console.Write("X")
                            Key = Console.ReadKey(True).Key
                            Select Case Key
                                Case 37                                         'Left arrow
                                    If SolidMap(CursorX - 1, CursorY) <> 1 Then
                                        If VisibilityMap(CursorX - 1, CursorY) <> 0 Then
                                            ConsoleOutput.RenderSquare(CursorX, CursorY)
                                            CursorX -= 1
                                        End If
                                    End If
                                Case 38                                         'Up arrow
                                    If SolidMap(CursorX, CursorY - 1) <> 1 Then
                                        If VisibilityMap(CursorX, CursorY - 1) <> 0 Then
                                            ConsoleOutput.RenderSquare(CursorX, CursorY)
                                            CursorY -= 1
                                        End If
                                    End If
                                Case 39                                         'Right arrow
                                    If SolidMap(CursorX + 1, CursorY) <> 1 Then
                                        If VisibilityMap(CursorX + 1, CursorY) <> 0 Then
                                            ConsoleOutput.RenderSquare(CursorX, CursorY)
                                            CursorX += 1
                                        End If
                                    End If
                                Case 40                                         'Down arrow
                                    If SolidMap(CursorX, CursorY + 1) <> 1 Then
                                        If VisibilityMap(CursorX, CursorY + 1) <> 0 Then
                                            ConsoleOutput.RenderSquare(CursorX, CursorY)
                                            CursorY += 1
                                        End If
                                    End If
                                Case 27                                         'Esc Key
                                    ConsoleOutput.RenderSquare(CursorX, CursorY)
                                    Console.SetCursorPosition(PosX, PosY)
                                    InteractChoiceMade = True
                                Case 13                                         'Enter Key

                                    ConsoleOutput.RenderSquare(CursorX, CursorY)
                                    Console.SetCursorPosition(PosX, PosY)
                                    InteractChoiceMade = True
                            End Select
                        End While
                    Case 27
                        'Code goes here for opening the Esc menu

                    Case 190        'Wait (.)
                        KeyHandled = True
                    Case 73 'i key
                        Call Player.ViewInventory()
                        Console.Clear()
                        ConsoleOutput.RenderMap()
                        ConsoleOutput.RenderGUI()
                        ConsoleOutput.RenderEnemies()
                        ConsoleOutput.RenderEventLog()
                    Case 65     'a
                        If Ranged = True Then
                            InteractChoiceMade = False
                            CursorX = PosX
                            CursorY = PosY
                            While InteractChoiceMade = False
                                Console.SetCursorPosition(PosX, PosY)
                                Console.BackgroundColor = TerrainLookupTable(1, TerrainMap(PosX, PosY))
                                'Select Case TerrainMap(PosX, PosY)
                                '    Case 0
                                'Console.BackgroundColor = ConsoleColor.Black
                                '    Case 1
                                'Console.BackgroundColor = ConsoleColor.Gray
                                '    Case 2
                                'Console.BackgroundColor = ConsoleColor.DarkGray
                                '    Case 3
                                'Console.BackgroundColor = ConsoleColor.DarkYellow
                                '    Case 10
                                'Console.BackgroundColor = ConsoleColor.Cyan
                                '    Case 11
                                'Console.BackgroundColor = ConsoleColor.DarkRed
                                'End Select
                                Console.Write("☺")
                                ConsoleOutput.RenderEnemies()
                                Console.SetCursorPosition(CursorX, CursorY)
                                Console.BackgroundColor = TerrainLookupTable(1, TerrainMap(CursorX, CursorY))
                                'Select Case TerrainMap(CursorX, CursorY)
                                '    Case 0
                                'Console.BackgroundColor = ConsoleColor.Black
                                '    Case 1
                                'Console.BackgroundColor = ConsoleColor.Gray
                                '    Case 2
                                'Console.BackgroundColor = ConsoleColor.DarkGray
                                '    Case 3
                                'Console.BackgroundColor = ConsoleColor.DarkYellow
                                '    Case 10
                                'Console.BackgroundColor = ConsoleColor.Cyan
                                '    Case 11
                                'Console.BackgroundColor = ConsoleColor.DarkRed
                                'End Select
                                Console.Write("X")
                                Key = Console.ReadKey(True).Key
                                Select Case Key
                                    Case 37                                         'Left arrow
                                        If SolidMap(CursorX - 1, CursorY) <> 1 Then
                                            If VisibilityMap(CursorX - 1, CursorY) <> 0 Then
                                                ConsoleOutput.RenderSquare(CursorX, CursorY)
                                                CursorX -= 1
                                            End If
                                        End If
                                    Case 38                                         'Up arrow
                                        If SolidMap(CursorX, CursorY - 1) <> 1 Then
                                            If VisibilityMap(CursorX, CursorY - 1) <> 0 Then
                                                ConsoleOutput.RenderSquare(CursorX, CursorY)
                                                CursorY -= 1
                                            End If
                                        End If
                                    Case 39                                         'Right arrow
                                        If SolidMap(CursorX + 1, CursorY) <> 1 Then
                                            If VisibilityMap(CursorX + 1, CursorY) <> 0 Then
                                                ConsoleOutput.RenderSquare(CursorX, CursorY)
                                                CursorX += 1
                                            End If
                                        End If
                                    Case 40                                         'Down arrow
                                        If SolidMap(CursorX, CursorY + 1) <> 1 Then
                                            If VisibilityMap(CursorX, CursorY + 1) <> 0 Then
                                                ConsoleOutput.RenderSquare(CursorX, CursorY)
                                                CursorY += 1
                                            End If
                                        End If
                                    Case 27                                         'Esc Key
                                        ConsoleOutput.RenderSquare(CursorX, CursorY)
                                        Console.SetCursorPosition(PosX, PosY)
                                        InteractChoiceMade = True
                                    Case 13                                         'Enter Key
                                        ConsoleOutput.RenderSquare(CursorX, CursorY)
                                        If Arrows > 0 Then      'If the player has at least 1 round of ammo for the currently equipped gun
                                            Bullet.X = PosX
                                            Bullet.Y = PosY
                                            Bullet.DestX = CursorX
                                            Bullet.DestY = CursorY
                                            Bullet.Type = 0
                                            Bullet.Fire()
                                            Arrows -= 1
                                            Console.SetCursorPosition(PosX, PosY)
                                            InteractChoiceMade = True
                                            KeyHandled = True
                                        Else
                                            EventlogSize += 1
                                            ReDim Preserve EventLog(EventlogSize)
                                            EventLog(EventlogSize) = "You reach for your quiver but find you are out of arrows!"
                                            ConsoleOutput.RenderEventLog()
                                            InteractChoiceMade = True
                                            KeyHandled = True
                                        End If
                                End Select
                            End While
                        End If
                End Select
            End While

            Entitymap(OldPosX, OldPosY) = 0
            Entitymap(PosX, PosY) = 1                                       'Marks the player on the entity map.

            Try
                Call LineOfSight(PosX, PosY)
                ConsoleOutput.RenderMap()
                Console.SetCursorPosition(OldPosX, OldPosY)
                ConsoleOutput.RenderSquare(OldPosX, OldPosY)
                Console.SetCursorPosition(PosX, PosY)


                Console.BackgroundColor = TerrainLookupTable(1, TerrainMap(PosX, PosY))
                'Select Case TerrainMap(PosX, PosY)
                '    Case 0
                'Console.BackgroundColor = ConsoleColor.Black
                '    Case 1
                'Console.BackgroundColor = ConsoleColor.Gray
                '    Case 2
                'Console.BackgroundColor = ConsoleColor.DarkGray
                '    Case 3
                'Console.BackgroundColor = ConsoleColor.DarkYellow
                '    Case 10
                'Console.BackgroundColor = ConsoleColor.Cyan
                '    Case 11
                'Console.BackgroundColor = ConsoleColor.DarkRed
                'End Select
                Console.ForegroundColor = ConsoleColor.White
                Console.Write("☺")
                Console.BackgroundColor = ConsoleColor.Black
            Catch ex As Exception
            End Try
            Console.SetCursorPosition(PosX, PosY)
        End Sub

        Sub PlayerCheckSquare(ByRef LevelID As Integer, ByVal DungeonID As Integer)
            Dim DungeonString As String
            Dim DungeonInfo() As String

            If NewlyEnteredDungeon = False Then
                If PosX = UpStair(0) Then
                    If PosY = UpStair(1) Then
                        If LevelID > 0 Then
                            Try
                                Call SaveMap(DungeonID, LevelID)
                                Call LoadMap(DungeonID, LevelID - 1)
                            Catch ex As Exception
                            End Try
                            PosX = DownStair(0)
                            PosY = DownStair(1)
                            Console.Clear()
                            ConsoleOutput.RenderMap()
                            Call LineOfSight(PosX, PosY)
                            ConsoleOutput.RenderMap()
                            Call ClearEntityMap()

                            DungeonString = GetDungeonInfo(DungeonID)
                            DungeonInfo = DungeonString.Split(";")
                            MaxEnemyLevel = 12 / ((Level + 1) / (DungeonInfo(2) + 1))

                            Call GenerateEnemies()
                            ConsoleOutput.RenderGUI()
                            ConsoleOutput.RenderEventLog()
                            LevelID -= 1
                            NewlyEnteredDungeon = True
                        Else
                            Call Overworld()
                        End If
                    End If
                End If
                If NewlyEnteredDungeon = False Then
                    If PosX = DownStair(0) Then
                        If PosY = DownStair(1) Then
                            Try
                                Call SaveMap(DungeonID, LevelID)
                                Call LoadMap(DungeonID, LevelID + 1)
                            Catch ex As Exception
                                Console.Clear()
                                Console.WriteLine("You have cleared the fortress!")
                                Select Case DungeonID
                                    Case 0
                                        Fort1Cleared = True
                                    Case 1
                                        Fort2Cleared = True
                                    Case 2
                                        Fort3Cleared = True
                                    Case 3
                                        Fort4Cleared = True
                                End Select
                                Console.ReadLine()
                                If Fort1Cleared = True Then
                                    If Fort2Cleared = True Then
                                        If Fort3Cleared = True Then
                                            If Fort4Cleared = True Then
                                                Console.Clear()
                                                Call GameWon()
                                            End If
                                        End If
                                    End If
                                End If
                                Call Overworld()
                            End Try
                            PosX = UpStair(0)
                            PosY = UpStair(1)
                            Console.Clear()
                            ConsoleOutput.RenderMap()
                            Call LineOfSight(PosX, PosY)
                            ConsoleOutput.RenderMap()
                            Call ClearEntityMap()

                            DungeonString = GetDungeonInfo(DungeonID)
                            DungeonInfo = DungeonString.Split(";")
                            MaxEnemyLevel = 12 / ((Level + 1) / (DungeonInfo(2) + 1))

                            Call GenerateEnemies()
                            ConsoleOutput.RenderGUI()
                            ConsoleOutput.RenderEventLog()
                            LevelID += 1
                            NewlyEnteredDungeon = True
                        End If
                    End If
                End If
            End If
        End Sub

        Sub Attacked(ByVal EnemyAttack, ByVal EnemyName)
            Dim Dice As Integer
            Dim Damage As Integer
            Dim Defence As Integer

            Dice = CInt((3 * Rnd()) + 1)

            Damage = Dice + EnemyAttack

            Defence = CInt(Me.Toughness / 3) + Me.Armour

            If Damage > 0 Then
                Me.Health -= Damage
                EventlogSize += 1
                ReDim Preserve EventLog(EventlogSize)
                EventLog(EventlogSize) = "The " & EnemyName & " hits you for " & Damage & " damage!"
                ConsoleOutput.RenderEventLog()
                ConsoleOutput.RenderGUI()
            Else
                EventlogSize += 1
                ReDim Preserve EventLog(EventlogSize)
                EventLog(EventlogSize) = "The " & EnemyName & "'s attack glances off of you!"
                ConsoleOutput.RenderEventLog()
            End If

            If Health <= 0 Then
                Me.Kill("a " & EnemyName)
            End If
        End Sub

        Sub Shot(ByVal EnemyName, ByVal EnemyAccuracy, ByVal EnemyAttack)
            Dim Dice As Integer
            Dim Damage As Integer
            Dim Defence As Integer

            Dice = ((12 * Rnd()) + 1)

            If Dice < EnemyAccuracy Then
                Dice = ((3 * Rnd()) + 1)
                Damage = Dice + EnemyAttack

                Defence = Me.Armour + (Me.Toughness / 3)

                Damage = Damage - Defence

                If Damage > 0 Then
                    Me.Health -= Damage
                    EventlogSize += 1
                    ReDim Preserve EventLog(EventlogSize)
                    EventLog(EventlogSize) = "The " & EnemyName & " shoots you for " & Damage & " damage!"
                    ConsoleOutput.RenderEventLog()
                Else
                    EventlogSize += 1
                    ReDim Preserve EventLog(EventlogSize)
                    EventLog(EventlogSize) = "The " & EnemyName & "'s shot glances off of you!"
                    ConsoleOutput.RenderEventLog()
                End If
            Else
                EventlogSize += 1
                ReDim Preserve EventLog(EventlogSize)
                EventLog(EventlogSize) = "The " & EnemyName & "'s shot misses you!"
                ConsoleOutput.RenderEventLog()
            End If
        End Sub

        Sub Kill(ByVal EnemyName)
            Console.Clear()
            ConsoleOutput.SlowWrite(1, 1, 150, "You are dead...")
            ConsoleOutput.SlowWrite(1, 3, 150, "You were killed by " & EnemyName & ".")
            ConsoleOutput.SlowWrite(1, 4, 150, "You lived for " & ((TimeAlive * 5) / 60) & " hours.")
            ConsoleOutput.SlowWrite(1, 5, 150, "You managed to slay " & KillCount & " enemies.")
            ConsoleOutput.SlowWrite(1, 7, 150, "Though your story is over, another hero awaits to take your place!")
            Console.ReadLine()
            Console.BackgroundColor = ConsoleColor.Black
            Console.ForegroundColor = ConsoleColor.White
            Call DeleteAllPlayerData()
            Call MainMenu()
        End Sub

        Sub UpdateHunger()
            If Me.TimeAlive >= Me.LastTimeHungerUpdated + 720 Then
                Me.Hunger -= 1
                Me.LastTimeHungerUpdated = Me.TimeAlive
            End If
            If Me.Hunger = 0 Then
                Me.Kill("Starvation")
            End If
        End Sub

        Sub ViewInventory()
            Dim SelectedItem As Integer = 0
            Dim OldNum As Integer = 0
            Dim Key As Integer
            Dim InnerKey As Integer
            Dim Choice As Integer
            Console.ForegroundColor = ConsoleColor.White
            Console.BackgroundColor = ConsoleColor.Black
            Console.Clear()

            UpdateInventory()

            For C = 0 To InventorySize
                Select Case Inventory(0, C)
                    Case 0
                        Console.Write(WeaponTable(0, Inventory(1, C)))
                    Case 1
                        Console.Write(ArmourTable(0, Inventory(1, C)))
                    Case 2
                        Console.Write(ConsumableTable(0, Inventory(1, C)))
                    Case 3
                        Console.Write(SpellTable(0, Inventory(1, C)))
                End Select
                Console.WriteLine(" x" & Inventory(2, C))
            Next

            While True
                Console.SetCursorPosition(0, OldNum)
                Console.Write("                                                         ")
                Console.SetCursorPosition(0, OldNum)
                Select Case Inventory(0, OldNum)
                    Case 0
                        Console.BackgroundColor = ConsoleColor.DarkRed
                        Console.Write(WeaponTable(0, Inventory(1, OldNum)))
                    Case 1
                        Console.BackgroundColor = ConsoleColor.Gray
                        Console.Write(ArmourTable(0, Inventory(1, OldNum)))
                    Case 2
                        Console.BackgroundColor = ConsoleColor.DarkYellow
                        Console.Write(ConsumableTable(0, Inventory(1, OldNum)))
                    Case 3
                        Console.BackgroundColor = ConsoleColor.DarkBlue
                        Console.Write(SpellTable(0, Inventory(1, OldNum)))
                End Select
                Console.Write(" x" & Inventory(2, OldNum))
                OldNum = SelectedItem
                Console.SetCursorPosition(0, SelectedItem)
                Console.BackgroundColor = ConsoleColor.White
                Select Case Inventory(0, SelectedItem)
                    Case 0
                        Console.ForegroundColor = ConsoleColor.DarkRed
                        Console.Write(WeaponTable(0, Inventory(1, SelectedItem)))
                    Case 1
                        Console.ForegroundColor = ConsoleColor.Gray
                        Console.Write(ArmourTable(0, Inventory(1, SelectedItem)))
                    Case 2
                        Console.ForegroundColor = ConsoleColor.DarkYellow
                        Console.Write(ConsumableTable(0, Inventory(1, SelectedItem)))
                    Case 3
                        Console.ForegroundColor = ConsoleColor.DarkBlue
                        Console.Write(SpellTable(0, Inventory(1, SelectedItem)))
                End Select
                Console.Write(" x" & Inventory(2, SelectedItem))
                Console.Write(" <")
                Console.BackgroundColor = ConsoleColor.Black
                Console.ForegroundColor = ConsoleColor.White
                Key = Console.ReadKey(True).Key

                Select Case Key

                    Case 38         'Down Arrow
                        If SelectedItem > 0 Then
                            SelectedItem -= 1
                        End If
                    Case 40         'Up Arrow
                        If SelectedItem < InventorySize Then
                            SelectedItem += 1
                        End If
                    Case 13         'Enter
                        Console.Clear()
                        Console.SetCursorPosition(1, 1)
                        Console.ForegroundColor = ConsoleColor.White
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.Write("Do what with the ")
                        Select Case Inventory(0, SelectedItem)
                            Case 0
                                Console.ForegroundColor = ConsoleColor.DarkRed
                                Console.Write(WeaponTable(0, Inventory(1, SelectedItem)))
                            Case 1
                                Console.ForegroundColor = ConsoleColor.Gray
                                Console.Write(ArmourTable(0, Inventory(1, SelectedItem)))
                            Case 2
                                Console.ForegroundColor = ConsoleColor.DarkYellow
                                Console.Write(ConsumableTable(0, Inventory(1, SelectedItem)))
                            Case 3
                                Console.ForegroundColor = ConsoleColor.DarkBlue
                                Console.Write(SpellTable(0, Inventory(1, SelectedItem)))
                        End Select
                        If Inventory(2, SelectedItem) > 1 Then
                            Console.Write("s")
                        End If
                        Console.ForegroundColor = ConsoleColor.White
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.Write("?")

                        Choice = 0

                        While True
                            Console.SetCursorPosition(1, 3)
                            If Choice = 0 Then
                                Console.BackgroundColor = ConsoleColor.White
                                Console.ForegroundColor = ConsoleColor.Black
                            End If
                            Select Case Inventory(0, SelectedItem)
                                Case 0
                                    Console.Write("Equip")
                                Case 1
                                    Console.Write("Equip")
                                Case 2
                                    Console.Write("Consume")
                                Case 3
                                    Console.Write("Attune")
                            End Select
                            Console.BackgroundColor = ConsoleColor.Black
                            Console.ForegroundColor = ConsoleColor.White
                            Console.SetCursorPosition(1, 4)
                            If Choice = 1 Then
                                Console.ForegroundColor = ConsoleColor.Black
                                Console.BackgroundColor = ConsoleColor.White
                            End If
                            Console.Write("Examine")
                            Console.BackgroundColor = ConsoleColor.Black
                            Console.ForegroundColor = ConsoleColor.White
                            Console.SetCursorPosition(1, 5)
                            If Choice = 2 Then
                                Console.ForegroundColor = ConsoleColor.Black
                                Console.BackgroundColor = ConsoleColor.White
                            End If
                            Console.Write("Destroy")
                            Console.BackgroundColor = ConsoleColor.Black
                            Console.ForegroundColor = ConsoleColor.White
                            InnerKey = Console.ReadKey(True).Key
                            Select Case InnerKey
                                Case 38 'Up arrow
                                    If Choice > 0 Then
                                        Choice -= 1
                                    End If
                                Case 40 'Down arrow
                                    If Choice < 2 Then
                                        Choice += 1
                                    End If
                                Case 13
                                    Select Case Choice
                                        Case 0
                                            Select Case Inventory(0, SelectedItem)
                                                Case 0
                                                    If WeaponTable(1, SelectedItem) <= Player.Strength Then
                                                        Inventory(2, SelectedItem) -= 1
                                                        Call EquipWeapon(Inventory(1, SelectedItem))
                                                        Call UpdateInventory()
                                                        SelectedItem = 0
                                                        Call ViewInventory()
                                                    Else
                                                        Console.Clear()
                                                        Console.WriteLine("You don't have enough strength to wield that!")
                                                        Console.ReadLine()
                                                        Call ViewInventory()
                                                    End If
                                                Case 1
                                                    If ArmourTable(1, SelectedItem) <= Player.Strength Then
                                                        Inventory(2, SelectedItem) -= 1
                                                        Call EquipArmour(Inventory(1, SelectedItem))
                                                        Call UpdateInventory()
                                                        SelectedItem = 0
                                                        Call ViewInventory()
                                                    Else
                                                        Console.Clear()
                                                        Console.WriteLine("You don't have enough strength to wear that!")
                                                        Console.ReadLine()
                                                        Call ViewInventory()
                                                    End If
                                                Case 2
                                                    Inventory(2, SelectedItem) -= 1
                                                    Call ConsumeItem(Inventory(1, SelectedItem))
                                                    Call UpdateInventory()
                                                    SelectedItem = 0
                                                    Call ViewInventory()
                                                Case 3
                                                    'Code goes here for equipping magic (not added yet)
                                                    SelectedItem = 0
                                                    Call ViewInventory()
                                            End Select
                                        Case 1
                                            Console.Clear()
                                            Select Case Inventory(0, SelectedItem)
                                                Case 0
                                                    Console.WriteLine("Name: " & WeaponTable(0, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Required Strength: " & WeaponTable(1, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Attack Bonus: " & WeaponTable(2, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("1 or 2 Handed: " & WeaponTable(3, Inventory(1, SelectedItem)) & " Handed")
                                                    Console.WriteLine("Weight: " & WeaponTable(4, Inventory(1, SelectedItem)) & "Kg")
                                                    Console.WriteLine("Value: §" & WeaponTable(5, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Description: " & WeaponTable(6, Inventory(1, SelectedItem)))
                                                Case 1
                                                    Console.WriteLine("Name: " & ArmourTable(0, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Required Strength: " & ArmourTable(1, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Defence Bonus: " & ArmourTable(2, Inventory(1, SelectedItem)))
                                                    Console.Write("Equip Location: ")
                                                    Select Case ArmourTable(3, Inventory(1, SelectedItem))
                                                        Case 0
                                                            Console.WriteLine("Head")
                                                        Case 1
                                                            Console.WriteLine("Torso")
                                                        Case 2
                                                            Console.WriteLine("Legs")
                                                        Case 3
                                                            Console.WriteLine("Arms")
                                                        Case 4
                                                            Console.WriteLine("Shield")
                                                    End Select
                                                    Console.WriteLine("Weight: " & ArmourTable(4, Inventory(1, SelectedItem)) & "Kg")
                                                    Console.WriteLine("Value: §" & ArmourTable(5, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Description: " & ArmourTable(6, Inventory(1, SelectedItem)))
                                                Case 2
                                                    Console.WriteLine("Name: " & ConsumableTable(0, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Health Bonus: " & ConsumableTable(1, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Mana Bonus: " & ConsumableTable(2, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Hunger Bonus: " & ConsumableTable(3, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Weight: " & ConsumableTable(4, Inventory(1, SelectedItem)) & "Kg")
                                                    Console.WriteLine("Value: §" & ConsumableTable(5, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Description: " & ConsumableTable(6, Inventory(1, SelectedItem)))
                                                Case 3
                                                    Console.WriteLine("Name: " & SpellTable(0, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Required Wisdom: " & SpellTable(1, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Mana Cost: " & SpellTable(2, Inventory(1, SelectedItem)))
                                                    Console.Write("Magic Type: ")
                                                    Select Case SpellTable(3, Inventory(1, SelectedItem))
                                                        Case 0
                                                            Console.WriteLine("Projectile")
                                                        Case 1
                                                            Console.WriteLine("Offensive Buff")
                                                        Case 2
                                                            Console.WriteLine("Defensive Buff")
                                                        Case 3
                                                            Console.WriteLine("Healing")
                                                        Case 4
                                                            Console.WriteLine("Summoning")
                                                    End Select
                                                    Console.WriteLine("Value: " & SpellTable(4, Inventory(1, SelectedItem)))
                                                    Console.WriteLine("Cooldown Period: " & SpellTable(5, Inventory(1, SelectedItem)) & " Turns")
                                                    Console.WriteLine("Description: " & SpellTable(6, Inventory(1, SelectedItem)))
                                            End Select
                                            Console.ReadLine()
                                            Call ViewInventory()
                                        Case 2
                                            Dim Temp As Integer
                                            Console.Clear()
                                            Console.SetCursorPosition(1, 1)
                                            Console.Write("Destroy how many? (Max: " & Inventory(2, SelectedItem) & "):")
                                            Temp = Console.ReadLine()
                                            If Temp > Inventory(2, SelectedItem) Then
                                                Call ViewInventory()
                                            Else
                                                If Temp > 0 Then
                                                    Inventory(2, SelectedItem) -= Temp
                                                    Call UpdateInventory()
                                                Else
                                                    Call ViewInventory()
                                                End If
                                            End If
                                    End Select
                                Case 27
                                    Exit Sub
                            End Select
                        End While
                    Case 27
                        Exit Sub
                End Select
            End While
        End Sub

        Sub EquipWeapon(ByVal WeaponID As Integer)
            Dim WeaponFound As Boolean = False
            Dim ShieldFound As Boolean = False
            If EquippedWeapon = "N" Then
                EquippedWeapon = WeaponID
                Return
            End If
            For C = 0 To InventorySize
                If Inventory(0, C) = 0 Then
                    If Inventory(1, C) = EquippedWeapon Then
                        WeaponFound = True
                        Inventory(2, C) += 1
                        If WeaponTable(3, WeaponID) = 2 Then
                            If Equipment(3) <> "N" Then
                                For C2 = 0 To InventorySize
                                    If Inventory(0, C2) = 1 Then
                                        If Inventory(1, C2) = Equipment(3) Then
                                            ShieldFound = True
                                            Inventory(2, C2) += 1
                                            Equipment(3) = "N"
                                            Exit For
                                        End If
                                    End If
                                Next
                                If ShieldFound = False Then
                                    InventorySize += 1
                                    ReDim Preserve Inventory(2, InventorySize)
                                    Inventory(0, InventorySize) = 1
                                    Inventory(1, InventorySize) = Equipment(3)
                                    Inventory(2, InventorySize) = 1
                                    Equipment(3) = "N"
                                End If
                            End If
                        End If
                        EquippedWeapon = WeaponID
                        Exit For
                    End If
                End If
            Next
            If WeaponFound = False Then
                InventorySize += 1
                ReDim Preserve Inventory(2, InventorySize)
                Inventory(0, InventorySize) = 0
                Inventory(1, InventorySize) = EquippedWeapon
                Inventory(2, InventorySize) = 1
                EquippedWeapon = WeaponID
            End If
        End Sub

        Sub EquipArmour(ByVal ArmourID As Integer)
            Dim ArmourFound As Boolean = False
            Dim WeaponFound As Boolean = False

            If Equipment(ArmourTable(3, ArmourID)) = "N" Then
                If ArmourTable(3, ArmourID) = 4 Then
                    If WeaponTable(3, EquippedWeapon) = 2 Then
                        For C = 0 To InventorySize
                            If Inventory(0, C) = 0 Then
                                If Inventory(1, C) = EquippedWeapon Then
                                    WeaponFound = True
                                    EquippedWeapon = "N"
                                    Inventory(2, C) += 1
                                    Equipment(4) = ArmourID
                                    Return
                                End If
                            End If
                        Next
                        If WeaponFound = False Then
                            InventorySize += 1
                            ReDim Preserve Inventory(2, InventorySize)
                            Inventory(0, InventorySize) = 0
                            Inventory(1, InventorySize) = EquippedWeapon
                            Inventory(2, InventorySize) = 1
                            EquippedWeapon = "N"
                            Equipment(4) = ArmourID
                            Return
                        End If
                    End If
                End If
                Equipment(ArmourTable(3, ArmourID)) = ArmourID
                Call CalculateNewArmourValue()
                Return
            Else
                For C = 0 To InventorySize
                    If Inventory(0, C) = 1 Then
                        If Inventory(1, C) = Equipment(ArmourTable(3, ArmourID)) Then
                            ArmourFound = True
                            Inventory(2, C) += 1
                            Equipment(ArmourTable(3, ArmourID)) = ArmourID
                            Exit For
                        End If
                    End If
                Next
                If ArmourFound = False Then
                    InventorySize += 1
                    ReDim Preserve Inventory(2, InventorySize)
                    Inventory(0, InventorySize) = 0
                    Inventory(1, InventorySize) = Equipment(ArmourTable(3, ArmourID))
                    Inventory(2, InventorySize) = 1
                    Equipment(ArmourTable(3, ArmourID)) = ArmourID
                    Return
                End If
            End If
        End Sub

        Sub ConsumeItem(ByVal ConsumableID As Integer)
            Player.Health += ConsumableTable(1, ConsumableID)
            If Player.Health > Player.MaxHealth Then
                Player.Health = Player.MaxHealth
            End If
            Player.Mana += ConsumableTable(2, ConsumableID)
            If Player.Mana > Player.MaxMana Then
                Player.Mana = Player.MaxMana
            End If
            Player.Hunger += ConsumableTable(3, ConsumableID)
            Player.LastTimeHungerUpdated = Player.TimeAlive
            If Player.Hunger > 20 Then
                Player.Hunger = 20
            End If
        End Sub

        Sub UpdateInventory()
            Dim TemporaryInventory(2, 0) As Integer
            Dim ElementsToRemove As Integer
            ReDim TemporaryInventory(2, InventorySize)
            For C = 0 To InventorySize
                TemporaryInventory(0, C) = Inventory(0, C)
                TemporaryInventory(1, C) = Inventory(1, C)
                TemporaryInventory(2, C) = Inventory(2, C)
                If TemporaryInventory(2, C) = 0 Then
                    ElementsToRemove += 1
                End If
            Next
            InventorySize -= ElementsToRemove
            ReDim Inventory(2, InventorySize)
            For C = 0 To InventorySize
                If TemporaryInventory(2, C) <> 0 Then
                    Inventory(0, C) = TemporaryInventory(0, C)
                    Inventory(1, C) = TemporaryInventory(1, C)
                    Inventory(2, C) = TemporaryInventory(2, C)
                End If
            Next
        End Sub

        Sub UpdateEquipmentWeight()

        End Sub

        Sub UpdateCarriedWeight()

        End Sub

        Sub CheckLevel()
            Dim ExperienceCarry As Integer
            If Experience(0) >= Experience(1) Then
                ExperienceCarry = Experience(0) - Experience(1)
                Me.Level += 1
                Call LevelUp()
                Experience(1) = Experience(1) * 2
                Experience(0) = ExperienceCarry
                Call CheckLevel()
            End If
        End Sub

        Sub LevelUp()
            Dim Choice As Integer
            Dim Key As Integer

            Console.Clear()
            Console.WriteLine("Congratulations! You reached level " & Me.Level & "!")
            Console.WriteLine("Choose a stat to level up:")

            While True
                Console.ForegroundColor = ConsoleColor.White
                Console.BackgroundColor = ConsoleColor.Black
                Console.SetCursorPosition(1, 3)
                Console.Write("Health: " & MaxHealth & "           ")
                Console.SetCursorPosition(1, 4)
                Console.Write("Mana: " & MaxMana & "           ")
                Console.SetCursorPosition(1, 5)
                Console.Write("Strength: " & Strength & "           ")
                Console.SetCursorPosition(1, 6)
                Console.Write("Toughness: " & Toughness & "           ")
                Console.SetCursorPosition(1, 7)
                Console.Write("Ranged Weapons: " & RangedWeapons & "           ")
                Console.SetCursorPosition(1, 8)
                Console.Write("Melee Weapons: " & MeleeWeapons & "           ")
                Console.SetCursorPosition(1, 9)
                Console.Write("Wisdom: " & Wisdom & "           ")
                Console.SetCursorPosition(1, 10)
                Console.Write("Medicine: " & Medicine & "           ")
                Console.SetCursorPosition(1, 11)
                Console.Write("Bartering: " & Bartering & "           ")

                Console.ForegroundColor = ConsoleColor.Black
                Console.BackgroundColor = ConsoleColor.White
                Console.SetCursorPosition(1, 3 + Choice)
                Select Case Choice
                    Case 0
                        Console.Write("Health: " & MaxHealth & " (+10)")
                    Case 1
                        Console.Write("Mana: " & MaxMana & " (+10)")
                    Case 2
                        Console.Write("Strength: " & Strength & " (+1)")
                    Case 3
                        Console.Write("Toughness: " & Toughness & " (+1)")
                    Case 4
                        Console.Write("Ranged Weapons: " & RangedWeapons & " (+1)")
                    Case 5
                        Console.Write("Melee Weapons: " & MeleeWeapons & " (+1)")
                    Case 6
                        Console.Write("Wisdom: " & Wisdom & " (+1)")
                    Case 7
                        Console.Write("Medicine: " & Medicine & " (+1)")
                    Case 8
                        Console.Write("Bartering: " & Bartering & " (+1)")
                End Select

                Key = Console.ReadKey(True).Key

                Select Case Key
                    Case 38
                        If Choice > 0 Then
                            Choice -= 1
                        End If
                    Case 40
                        If Choice < 8 Then
                            Choice += 1
                        End If
                    Case 13
                        Select Case Choice
                            Case 0
                                MaxHealth += 10
                                Health = MaxHealth
                            Case 1
                                MaxMana += 10
                                Mana = MaxMana
                            Case 2
                                Strength += 1
                            Case 3
                                Toughness += 1
                            Case 4
                                RangedWeapons += 1
                            Case 5
                                MeleeWeapons += 1
                            Case 6
                                Wisdom += 1
                            Case 7
                                Medicine += 1
                            Case 8
                                Bartering += 1
                        End Select
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.ForegroundColor = ConsoleColor.White
                        Console.Clear()
                        ConsoleOutput.RenderGUI()
                        ConsoleOutput.RenderMap()
                        ConsoleOutput.RenderEnemies()
                        Exit Sub
                End Select

            End While
        End Sub

    End Class

#Region "Runtime Management"

    Sub NewGame()
        My.Computer.Audio.Stop()
        My.Computer.Audio.Play(My.Resources.MeetingWithAGod, AudioPlayMode.BackgroundLoop)
        Dim ChoiceString As String
        Dim KeyInput As Integer

        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Green
        Console.Clear()
        Console.CursorVisible = False

        Console.BackgroundColor = ConsoleColor.DarkGray
        Console.ForegroundColor = ConsoleColor.DarkGray

        For Y = 0 To 59
            Console.SetCursorPosition(0, Y)
            Console.Write("█")
            Console.SetCursorPosition(89, Y)
            Console.Write("█")
        Next

        For X = 0 To 89
            Console.SetCursorPosition(X, 0)
            Console.Write("█")
            Console.SetCursorPosition(X, 59)
            Console.Write("█")
        Next

        Console.ForegroundColor = ConsoleColor.Black

        For Y = 1 To 58
            Console.SetCursorPosition(1, Y)
            Console.Write("▒")
            Console.SetCursorPosition(88, Y)
            Console.Write("▒")
        Next

        For X = 1 To 88
            Console.SetCursorPosition(X, 1)
            Console.Write("▒")
            Console.SetCursorPosition(X, 58)
            Console.Write("▒")
        Next

        Console.SetCursorPosition(0, 0)

        Console.BackgroundColor = ConsoleColor.Black

        ConsoleOutput.SlowWrite(3, 3, 125, "Hello?")
        System.Threading.Thread.Sleep(2000)
        ConsoleOutput.SlowWrite(3, 3, 125, "Ah, good, you're asleep.")
        System.Threading.Thread.Sleep(2500)
        ConsoleOutput.SlowWrite(3, 3, 125, "Oh, no need to wake up, you stay right wherever it is that you are.")
        System.Threading.Thread.Sleep(4000)
        ConsoleOutput.SlowWrite(3, 3, 125, "It's far easier for me this way, I can assure you.                 ")
        System.Threading.Thread.Sleep(3000)
        ConsoleOutput.SlowWrite(3, 3, 125, "We gods aren't always mad about shock and awe.                     ")
        System.Threading.Thread.Sleep(2500)
        ConsoleOutput.SlowWrite(3, 3, 125, "Right, i'll cut to the chase. I have a proposition for you.        ")
        System.Threading.Thread.Sleep(3000)
        ConsoleOutput.SlowWrite(3, 3, 125, "Oh my, where are my manners! I completely forgot to introduce myself.")
        System.Threading.Thread.Sleep(3500)
        ConsoleOutput.SlowWrite(3, 3, 125, "I am, in your tongue, Gaebrenor, God of humanity.                    ")
        System.Threading.Thread.Sleep(3000)
        ConsoleOutput.SlowWrite(3, 3, 125, "While the other gods might worry about volcanoes, fertility or hats,")
        System.Threading.Thread.Sleep(1000)
        ConsoleOutput.SlowWrite(3, 4, 125, "I like to concern myself with the fate of humans.                 ")
        System.Threading.Thread.Sleep(5000)
        Console.SetCursorPosition(3, 4)
        Console.Write("                                                             ")
        ConsoleOutput.SlowWrite(3, 3, 125, "Frankly, you're in dire straights.                                  ")
        System.Threading.Thread.Sleep(2500)
        ConsoleOutput.SlowWrite(3, 3, 125, "But enough of that, i'm forgetting my conduct once again.           ")
        System.Threading.Thread.Sleep(3000)
        ConsoleOutput.SlowWrite(3, 3, 125, "Pray tell, who are you?                                             ")

        Call InputName()

        Console.SetCursorPosition(3, 5)
        Console.Write("                                                                              ")
        Console.SetCursorPosition(3, 7)
        Console.Write("                                                                       ")
        Console.SetCursorPosition(3, 8)
        Console.Write("                                                                       ")

        ConsoleOutput.SlowWrite(3, 3, 125, "Well then " & Player.Name & ",               ")
        System.Threading.Thread.Sleep(500)
        ConsoleOutput.SlowWrite(3, 4, 125, "Would you like to hear a tale of the land of Eudai?")

        ChoiceString = "Yes"

        While True
            ConsoleOutput.SlowWrite(3, 6, 100, ChoiceString & "   ")
            KeyInput = Console.ReadKey(True).Key
            Select Case KeyInput
                Case 38
                    If ChoiceString = "No" Then
                        ChoiceString = "Yes"
                    End If
                Case 40
                    If ChoiceString = "Yes" Then
                        ChoiceString = "No"
                    End If
                Case 13
                    Exit While
            End Select
        End While

        If ChoiceString = "Yes" Then
            Call BackStory()
        Else
            Console.SetCursorPosition(3, 6)
            Console.Write("                                                                              ")
            Console.SetCursorPosition(3, 3)
            Console.Write("                                                                       ")
            Console.SetCursorPosition(3, 4)
            Console.Write("                                                                       ")
            ConsoleOutput.SlowWrite(3, 3, 125, "Ah, straight to the point I see. Very well, let us continue!        ")
            System.Threading.Thread.Sleep(3000)
            Call CharacterCreation()
        End If
        Console.ReadLine()
        Call NewGameMenu()
    End Sub

    Sub BackStory()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Green
        Console.Clear()
        Console.CursorVisible = False

        Console.BackgroundColor = ConsoleColor.DarkGray
        Console.ForegroundColor = ConsoleColor.DarkGray

        For Y = 0 To 59
            Console.SetCursorPosition(0, Y)
            Console.Write("█")
            Console.SetCursorPosition(89, Y)
            Console.Write("█")
        Next

        For X = 0 To 89
            Console.SetCursorPosition(X, 0)
            Console.Write("█")
            Console.SetCursorPosition(X, 59)
            Console.Write("█")
        Next

        Console.ForegroundColor = ConsoleColor.Black

        For Y = 1 To 58
            Console.SetCursorPosition(1, Y)
            Console.Write("▒")
            Console.SetCursorPosition(88, Y)
            Console.Write("▒")
        Next

        For X = 1 To 88
            Console.SetCursorPosition(X, 1)
            Console.Write("▒")
            Console.SetCursorPosition(X, 58)
            Console.Write("▒")
        Next

        Console.SetCursorPosition(0, 0)

        Console.BackgroundColor = ConsoleColor.Black

        'Backstory goes here

        Call CharacterCreation()

    End Sub

    Sub CharacterCreation()
        Dim KeyChoice As Integer
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Green
        Console.Clear()
        Console.CursorVisible = False

        Console.BackgroundColor = ConsoleColor.DarkGray
        Console.ForegroundColor = ConsoleColor.DarkGray

        For Y = 0 To 59
            Console.SetCursorPosition(0, Y)
            Console.Write("█")
            Console.SetCursorPosition(89, Y)
            Console.Write("█")
        Next

        For X = 0 To 89
            Console.SetCursorPosition(X, 0)
            Console.Write("█")
            Console.SetCursorPosition(X, 59)
            Console.Write("█")
        Next

        Console.ForegroundColor = ConsoleColor.Black

        For Y = 1 To 58
            Console.SetCursorPosition(1, Y)
            Console.Write("▒")
            Console.SetCursorPosition(88, Y)
            Console.Write("▒")
        Next

        For X = 1 To 88
            Console.SetCursorPosition(X, 1)
            Console.Write("▒")
            Console.SetCursorPosition(X, 58)
            Console.Write("▒")
        Next

        Console.SetCursorPosition(0, 0)

        Console.BackgroundColor = ConsoleColor.Black

        ConsoleOutput.SlowWrite(3, 3, 125, "Now, your task is quite simple really.")
        System.Threading.Thread.Sleep(3000)
        ConsoleOutput.SlowWrite(3, 3, 125, "I am going to send you to Eudai.      ")
        System.Threading.Thread.Sleep(2500)
        ConsoleOutput.SlowWrite(3, 3, 125, "Once you are there, you must slay the four kings.")
        System.Threading.Thread.Sleep(3000)
        ConsoleOutput.SlowWrite(3, 3, 125, "Isrim, king of fire                              ")
        System.Threading.Thread.Sleep(2000)
        ConsoleOutput.SlowWrite(3, 4, 125, "Arallis, king of air")
        System.Threading.Thread.Sleep(2000)
        ConsoleOutput.SlowWrite(3, 5, 125, "Phyadarin, king of water")
        System.Threading.Thread.Sleep(2000)
        ConsoleOutput.SlowWrite(3, 6, 125, "Jokmar, king of earth")
        System.Threading.Thread.Sleep(3000)
        Console.SetCursorPosition(3, 4)
        Console.Write("                                        ")
        Console.SetCursorPosition(3, 5)
        Console.Write("                                        ")
        Console.SetCursorPosition(3, 6)
        Console.Write("                                        ")
        ConsoleOutput.SlowWrite(3, 3, 125, "It is imperative that you accomplish this mission. ")
        System.Threading.Thread.Sleep(3000)
        ConsoleOutput.SlowWrite(3, 3, 125, "The fate of every universe hangs in the balance.   ")
        System.Threading.Thread.Sleep(3000)
        ConsoleOutput.SlowWrite(3, 3, 125, "Alas, I have no power in Eudai. No god does.       ")
        System.Threading.Thread.Sleep(3000)
        ConsoleOutput.SlowWrite(3, 3, 125, "Thus, I must send you in my stead.                 ")
        System.Threading.Thread.Sleep(3000)
        ConsoleOutput.SlowWrite(3, 3, 125, "I can give you a strong physical form and useful equipment,")
        System.Threading.Thread.Sleep(1000)
        ConsoleOutput.SlowWrite(3, 4, 125, "But i'm afraid that is all I am able to do.")
        System.Threading.Thread.Sleep(4000)
        Console.SetCursorPosition(3, 4)
        Console.Write("                                                         ")
        ConsoleOutput.SlowWrite(3, 3, 125, "So, what type of fighter are you?                          ")
        System.Threading.Thread.Sleep(1000)

        Player.Role = "Warrior"

        While True
            Console.SetCursorPosition(3, 5)
            Console.Write(Player.Role & "          ")
            Select Case Player.Role
                Case "Warrior"
                    Player.MaxHealth = 100
                    Player.Health = 100
                    Player.MaxMana = 30
                    Player.Mana = 30
                    Player.Strength = 5
                    Player.Toughness = 5
                    Player.RangedWeapons = 4
                    Player.MeleeWeapons = 6
                    Player.Wisdom = 3
                    Player.Medicine = 5
                    Player.Bartering = 4
                    Player.Gold = 200
                    ConsoleOutput.SlowWrite(3, 7, 50, "   Health: ██████████  100     ")
                    ConsoleOutput.SlowWrite(3, 8, 50, "     Mana: ▒▒▒ 30              ")
                    ConsoleOutput.SlowWrite(3, 9, 50, " Strength: █████ 5             ")
                    ConsoleOutput.SlowWrite(3, 10, 50, "Toughness: ▒▒▒▒▒ 5            ")
                    ConsoleOutput.SlowWrite(3, 11, 50, "   Ranged: ████ 4             ")
                    ConsoleOutput.SlowWrite(3, 12, 50, "    Melee: ▒▒▒▒▒▒ 6           ")
                    ConsoleOutput.SlowWrite(3, 13, 50, "   Wisdom: ███ 3              ")
                    ConsoleOutput.SlowWrite(3, 14, 50, " Medicine: ▒▒▒▒▒ 5            ")
                    ConsoleOutput.SlowWrite(3, 15, 50, "Bartering: ████ 4             ")
                    ConsoleOutput.SlowWrite(3, 17, 50, "Warriors are good all-round fighters. They don't excel in any particular")
                    ConsoleOutput.SlowWrite(3, 18, 50, "field, but their broad range of skills allows them to fulfil multiple   ")
                    ConsoleOutput.SlowWrite(3, 19, 50, "roles. A good balanced class.                                           ")
                    ConsoleOutput.SlowWrite(3, 21, 50, "Starting Gear:")
                    ConsoleOutput.SlowWrite(3, 22, 50, "   - Light Iron Armour Set  ")
                    ConsoleOutput.SlowWrite(3, 23, 50, "   - Shortsword             ")
                    ConsoleOutput.SlowWrite(3, 24, 50, "   - Iron Buckler           ")
                    ConsoleOutput.SlowWrite(3, 25, 50, "   - Four Rations of Food   ")
                    ConsoleOutput.SlowWrite(3, 26, 50, "   - 200 Gold Pieces        ")
                    ConsoleOutput.SlowWrite(3, 27, 50, "                            ")

                Case "Mage"
                    Player.MaxHealth = 80
                    Player.Health = 80
                    Player.MaxMana = 100
                    Player.Mana = 100
                    Player.Strength = 3
                    Player.Toughness = 3
                    Player.RangedWeapons = 2
                    Player.MeleeWeapons = 2
                    Player.Wisdom = 8
                    Player.Medicine = 6
                    Player.Bartering = 3
                    Player.Gold = 200
                    ConsoleOutput.SlowWrite(3, 7, 50, "   Health: ████████  80        ")
                    ConsoleOutput.SlowWrite(3, 8, 50, "     Mana: ▒▒▒▒▒▒▒▒▒▒ 100      ")
                    ConsoleOutput.SlowWrite(3, 9, 50, " Strength: ███ 3               ")
                    ConsoleOutput.SlowWrite(3, 10, 50, "Toughness: ▒▒▒ 3              ")
                    ConsoleOutput.SlowWrite(3, 11, 50, "   Ranged: ██ 2               ")
                    ConsoleOutput.SlowWrite(3, 12, 50, "    Melee: ▒▒ 2               ")
                    ConsoleOutput.SlowWrite(3, 13, 50, "   Wisdom: ████████ 8         ")
                    ConsoleOutput.SlowWrite(3, 14, 50, " Medicine: ▒▒▒▒▒▒ 6           ")
                    ConsoleOutput.SlowWrite(3, 15, 50, "Bartering: ███ 3              ")
                    ConsoleOutput.SlowWrite(3, 17, 50, "Mages are masters of the arcane arts, capable of crushing entire armies ")
                    ConsoleOutput.SlowWrite(3, 18, 50, "with little more than a wave of their hand. However, their reliance on  ")
                    ConsoleOutput.SlowWrite(3, 19, 50, "magic leaves them more vulnerable to hand-to-hand combat.               ")
                    ConsoleOutput.SlowWrite(3, 21, 50, "Starting Gear:")
                    ConsoleOutput.SlowWrite(3, 22, 50, "   - Mage Robes Set         ")
                    ConsoleOutput.SlowWrite(3, 23, 50, "   - Staff                  ")
                    ConsoleOutput.SlowWrite(3, 24, 50, "   - Spell: Magic Missile   ")
                    ConsoleOutput.SlowWrite(3, 25, 50, "   - Four Rations of Food   ")
                    ConsoleOutput.SlowWrite(3, 26, 50, "   - 200 Gold Pieces        ")
                    ConsoleOutput.SlowWrite(3, 27, 50, "                            ")

                Case "Knight"
                    Player.MaxHealth = 150
                    Player.Health = 150
                    Player.MaxMana = 0
                    Player.Mana = 0
                    Player.Strength = 8
                    Player.Toughness = 8
                    Player.RangedWeapons = 0
                    Player.MeleeWeapons = 9
                    Player.Wisdom = 0
                    Player.Medicine = 2
                    Player.Bartering = 3
                    Player.Gold = 200
                    ConsoleOutput.SlowWrite(3, 7, 50, "   Health: ███████████████  150")
                    ConsoleOutput.SlowWrite(3, 8, 50, "     Mana: 0                   ")
                    ConsoleOutput.SlowWrite(3, 9, 50, " Strength: ████████ 8          ")
                    ConsoleOutput.SlowWrite(3, 10, 50, "Toughness: ▒▒▒▒▒▒▒▒ 8         ")
                    ConsoleOutput.SlowWrite(3, 11, 50, "   Ranged:  0                 ")
                    ConsoleOutput.SlowWrite(3, 12, 50, "    Melee: ▒▒▒▒▒▒▒▒▒ 9        ")
                    ConsoleOutput.SlowWrite(3, 13, 50, "   Wisdom:  0                 ")
                    ConsoleOutput.SlowWrite(3, 14, 50, " Medicine: ▒▒ 2               ")
                    ConsoleOutput.SlowWrite(3, 15, 50, "Bartering: ████ 3             ")
                    ConsoleOutput.SlowWrite(3, 17, 50, "A Knight is a towering figure of strength, honour and endurance.        ")
                    ConsoleOutput.SlowWrite(3, 18, 50, "Forsaking magic and ranged weapons, a Knight is unmatched when it comes ")
                    ConsoleOutput.SlowWrite(3, 19, 50, "to close combat.                                                        ")
                    ConsoleOutput.SlowWrite(3, 21, 50, "Starting Gear:")
                    ConsoleOutput.SlowWrite(3, 22, 50, "   - Iron Armour Set        ")
                    ConsoleOutput.SlowWrite(3, 23, 50, "   - Iron Greatsword        ")
                    ConsoleOutput.SlowWrite(3, 24, 50, "   - Five Rations of Food   ")
                    ConsoleOutput.SlowWrite(3, 25, 50, "   - 200 Gold Pieces        ")
                    ConsoleOutput.SlowWrite(3, 26, 50, "                            ")
                    ConsoleOutput.SlowWrite(3, 27, 50, "                            ")

                Case "Cleric"
                    Player.MaxHealth = 100
                    Player.Health = 100
                    Player.MaxMana = 50
                    Player.Mana = 50
                    Player.Strength = 5
                    Player.Toughness = 5
                    Player.RangedWeapons = 2
                    Player.MeleeWeapons = 5
                    Player.Wisdom = 4
                    Player.Medicine = 8
                    Player.Bartering = 1
                    Player.Gold = 50
                    ConsoleOutput.SlowWrite(3, 7, 50, "   Health: ██████████  100     ")
                    ConsoleOutput.SlowWrite(3, 8, 50, "     Mana: ▒▒▒▒▒ 50            ")
                    ConsoleOutput.SlowWrite(3, 9, 50, " Strength: █████ 5             ")
                    ConsoleOutput.SlowWrite(3, 10, 50, "Toughness: ▒▒▒▒▒ 5            ")
                    ConsoleOutput.SlowWrite(3, 11, 50, "   Ranged: ██ 2               ")
                    ConsoleOutput.SlowWrite(3, 12, 50, "    Melee: ▒▒▒▒▒ 5            ")
                    ConsoleOutput.SlowWrite(3, 13, 50, "   Wisdom: ████ 4             ")
                    ConsoleOutput.SlowWrite(3, 14, 50, " Medicine: ▒▒▒▒▒▒▒▒ 8         ")
                    ConsoleOutput.SlowWrite(3, 15, 50, "Bartering: █ 1                ")
                    ConsoleOutput.SlowWrite(3, 17, 50, "Clerics are strong warriors with a decent knowledge of magic, mainly    ")
                    ConsoleOutput.SlowWrite(3, 18, 50, "spells of healing and restoration. However, their lives of isolation    ")
                    ConsoleOutput.SlowWrite(3, 19, 50, "leave them with poor skills at bartering.                               ")
                    ConsoleOutput.SlowWrite(3, 21, 50, "Starting Gear:")
                    ConsoleOutput.SlowWrite(3, 22, 50, "   - Hard Leather Armour Set")
                    ConsoleOutput.SlowWrite(3, 23, 50, "   - Morning Star           ")
                    ConsoleOutput.SlowWrite(3, 24, 50, "   - Tower Shield           ")
                    ConsoleOutput.SlowWrite(3, 25, 50, "   - Spell: Healing         ")
                    ConsoleOutput.SlowWrite(3, 26, 50, "   - Four Rations of Food   ")
                    ConsoleOutput.SlowWrite(3, 27, 50, "   - 50 Gold Pieces         ")

                Case "Archer"
                    Player.MaxHealth = 150
                    Player.Health = 150
                    Player.MaxMana = 0
                    Player.Mana = 0
                    Player.Strength = 8
                    Player.Toughness = 8
                    Player.RangedWeapons = 0
                    Player.MeleeWeapons = 9
                    Player.Wisdom = 0
                    Player.Medicine = 2
                    Player.Bartering = 3
                    Player.Gold = 250
                    ConsoleOutput.SlowWrite(3, 7, 50, "   Health: █████████  90       ")
                    ConsoleOutput.SlowWrite(3, 8, 50, "     Mana:  0                  ")
                    ConsoleOutput.SlowWrite(3, 9, 50, " Strength: █████ 5             ")
                    ConsoleOutput.SlowWrite(3, 10, 50, "Toughness: ▒▒▒▒▒ 5            ")
                    ConsoleOutput.SlowWrite(3, 11, 50, "   Ranged: █████████ 9        ")
                    ConsoleOutput.SlowWrite(3, 12, 50, "    Melee: ▒▒▒ 3              ")
                    ConsoleOutput.SlowWrite(3, 13, 50, "   Wisdom:  0                 ")
                    ConsoleOutput.SlowWrite(3, 14, 50, " Medicine: ▒▒▒▒▒▒ 6           ")
                    ConsoleOutput.SlowWrite(3, 15, 50, "Bartering: ████████ 8         ")
                    ConsoleOutput.SlowWrite(3, 17, 50, "The arhcer picks off foes from a great distance, quickly and            ")
                    ConsoleOutput.SlowWrite(3, 18, 50, "efficiently. Usually charismatic, archers will be able to get better    ")
                    ConsoleOutput.SlowWrite(3, 19, 50, "prices in shops.                                                        ")
                    ConsoleOutput.SlowWrite(3, 21, 50, "Starting Gear:")
                    ConsoleOutput.SlowWrite(3, 22, 50, "   - Leather Armour Set     ")
                    ConsoleOutput.SlowWrite(3, 23, 50, "   - Shortbow               ")
                    ConsoleOutput.SlowWrite(3, 24, 50, "   - Dagger                 ")
                    ConsoleOutput.SlowWrite(3, 25, 50, "   - 100 Iron Arrows        ")
                    ConsoleOutput.SlowWrite(3, 26, 50, "   - Four Rations of Food   ")
                    ConsoleOutput.SlowWrite(3, 27, 50, "   - 250 Gold Pieces        ")

            End Select

            KeyChoice = Console.ReadKey(True).Key

            Select Case KeyChoice
                Case 38
                    If Player.Role = "Archer" Then
                        Player.Role = "Cleric"
                    ElseIf Player.Role = "Cleric" Then
                        Player.Role = "Knight"
                    ElseIf Player.Role = "Knight" Then
                        Player.Role = "Mage"
                    ElseIf Player.Role = "Mage" Then
                        Player.Role = "Warrior"
                    End If
                Case 40
                    If Player.Role = "Warrior" Then
                        Player.Role = "Mage"
                    ElseIf Player.Role = "Mage" Then
                        Player.Role = "Knight"
                    ElseIf Player.Role = "Knight" Then
                        Player.Role = "Cleric"
                    ElseIf Player.Role = "Cleric" Then
                        Player.Role = "Archer"
                    End If
                Case 13
                    Exit While
            End Select
        End While

        Select Case Player.Role
            Case "Warrior"
                Player.EquippedWeapon = 0   'Short-sword
                Player.Equipment(0) = 1     'Cervelliere
                Player.Equipment(1) = 10    'Iron Hauberk
                Player.Equipment(2) = 18    'Iron Chausses
                Player.Equipment(3) = 25    'Iron Vambraces
                Player.Equipment(4) = 34    'Iron Buckler
                Player.InventorySize = 0
                ReDim Player.Inventory(2, Player.InventorySize)
                Player.Inventory(0, 0) = 3
                Player.Inventory(1, 0) = 1
                Player.Inventory(2, 0) = 4
        End Select

        Player.Level = 1
        Player.Experience(0) = 0
        Player.Experience(1) = 100
        Player.Fort1Cleared = False
        Player.Fort2Cleared = False
        Player.Fort3Cleared = False
        Player.Fort4Cleared = False
        Player.Hunger = 20

        Call GenerateWorld(0)
    End Sub

    Sub InputName()
        Console.CursorVisible = True
        ConsoleOutput.SlowWrite(3, 7, 125, "[Simply tpye in your desired name then press enter!]")
        Console.SetCursorPosition(3, 5)
        Player.Name = Console.ReadLine
        If Player.Name.Length > 60 Then
            Console.SetCursorPosition(3, 5)
            Console.Write("                                                                             ")
            Call InputName()
        End If
        Console.CursorVisible = False
        Call InputGender()
    End Sub

    Sub InputGender()
        Dim Key As Integer
        Player.Gender = 0
        ConsoleOutput.SlowWrite(3, 7, 125, "[Now, select your gender using the up and down arrow keys and enter.]")
        ConsoleOutput.SlowWrite(3, 8, 125, "[If you entered the wrong name, simply press backspace!]")
        While True
            Console.SetCursorPosition(4 + Player.Name.Length, 5)
            If Player.Gender = 0 Then
                Console.Write("♂")
            ElseIf Player.Gender = 1 Then
                Console.Write("♀")
            End If
            Key = Console.ReadKey(True).Key
            Select Case Key
                Case 38
                    If Player.Gender = 1 Then
                        Player.Gender = 0
                    End If
                Case 40
                    If Player.Gender = 0 Then
                        Player.Gender = 1
                    End If
                Case 13
                    Exit While
                Case 8
                    Console.SetCursorPosition(3, 5)
                    Console.Write("                                                                              ")
                    Console.SetCursorPosition(3, 7)
                    Console.Write("                                                                       ")
                    Console.SetCursorPosition(3, 8)
                    Console.Write("                                                                       ")
                    Call InputName()
            End Select
        End While
        Return
    End Sub

    Sub NewGameMenu()
        Dim Choice As Integer
        Dim InnerChoice As Integer
        Dim ChoiceMade As Boolean = False
        Dim RoleSelection As Integer
        Console.Clear()
        Console.SetCursorPosition(0, 0)
        Console.WriteLine("╒════════════════════════════════════════════════════════════════════════════════════════╕")
        For Y As Integer = 1 To 58
            Console.SetCursorPosition(0, Y)
            Console.Write("│")
            Console.SetCursorPosition(89, Y)
            Console.Write("│")
            System.Threading.Thread.Sleep(5)
        Next
        Console.SetCursorPosition(0, 59)
        Console.Write("╘════════════════════════════════════════════════════════════════════════════════════════╛")
        Console.SetCursorPosition(0, 0)
        ConsoleOutput.SlowWrite(1, 1, 5, "Tacton Systems Neural Interface V2.94")
        ConsoleOutput.SlowWrite(1, 4, 5, "Neural Hub Main Menu:")
        ConsoleOutput.SlowWrite(1, 6, 5, "A - Commence Cryostasis Revival Process")
        ConsoleOutput.SlowWrite(1, 7, 5, "B - Help Centre")
        ConsoleOutput.SlowWrite(1, 8, 5, "C - Return to Cryogenic Hibernation")
        ConsoleOutput.SlowWrite(70, 45, 5, " █████  █████ ")
        ConsoleOutput.SlowWrite(70, 46, 5, "██████████████")
        ConsoleOutput.SlowWrite(70, 47, 5, "██   ▒██▒   ██")
        ConsoleOutput.SlowWrite(70, 48, 5, "    ▒▒██▒▒    ")
        ConsoleOutput.SlowWrite(70, 49, 5, "   ▒▒ ██ ▒▒   ")
        ConsoleOutput.SlowWrite(70, 50, 5, "  ▒▒▒▒██▒▒▒▒")
        ConsoleOutput.SlowWrite(70, 51, 5, " ▒▒   ██   ▒▒ ")
        ConsoleOutput.SlowWrite(70, 52, 5, "▒▒    ██    ▒▒")
        ConsoleOutput.SlowWrite(70, 53, 5, "     ████     ")
        ConsoleOutput.SlowWrite(70, 54, 5, "  ██████████  ")
        ConsoleOutput.SlowWrite(70, 55, 5, "  ████  ████  ")
        ConsoleOutput.SlowWrite(74, 57, 5, "TACTON")
        Console.CursorVisible = False
        Console.SetCursorPosition(0, 0)

        Choice = Console.ReadKey(True).Key

        Select Case Choice
            Case 65         'a
                ConsoleOutput.SlowWrite(1, 4, 1, "                      ")
                ConsoleOutput.SlowWrite(1, 6, 1, "                                        ")
                ConsoleOutput.SlowWrite(1, 7, 1, "               ")
                ConsoleOutput.SlowWrite(1, 8, 1, "                                    ")
                ConsoleOutput.SlowWrite(1, 4, 5, "Missing or Corrupted Records")
                ConsoleOutput.SlowWrite(1, 6, 5, "Due to unforseen circumstances, Tacton has missing data regarding your personal")
                ConsoleOutput.SlowWrite(1, 7, 5, "information, such as your name and occupation. It is necessary for you to provide")
                ConsoleOutput.SlowWrite(1, 8, 5, "this data before commencing the cryostasis revival process.")
                ConsoleOutput.SlowWrite(1, 10, 5, "┌────────────────────────────────────────┐")
                ConsoleOutput.SlowWrite(1, 11, 5, "│Name:")
                ConsoleOutput.SlowWrite(42, 11, 5, "│")
                ConsoleOutput.SlowWrite(1, 12, 5, "├────────────────────────────────────────┤")
                ConsoleOutput.SlowWrite(1, 13, 5, "│Occupation:")
                ConsoleOutput.SlowWrite(42, 13, 5, "│")
                ConsoleOutput.SlowWrite(1, 14, 5, "├────────────────────────────────────────┤")
                ConsoleOutput.SlowWrite(1, 15, 5, "│Job Description:")
                ConsoleOutput.SlowWrite(42, 15, 5, "│")
                ConsoleOutput.SlowWrite(1, 16, 5, "│")
                ConsoleOutput.SlowWrite(42, 16, 5, "│")
                ConsoleOutput.SlowWrite(1, 17, 5, "│")
                ConsoleOutput.SlowWrite(42, 17, 5, "│")
                ConsoleOutput.SlowWrite(1, 18, 5, "│")
                ConsoleOutput.SlowWrite(42, 18, 5, "│")
                ConsoleOutput.SlowWrite(1, 19, 5, "│")
                ConsoleOutput.SlowWrite(42, 19, 5, "│")
                ConsoleOutput.SlowWrite(1, 20, 5, "│")
                ConsoleOutput.SlowWrite(42, 20, 5, "│")
                ConsoleOutput.SlowWrite(1, 21, 5, "│")
                ConsoleOutput.SlowWrite(42, 21, 5, "│")
                ConsoleOutput.SlowWrite(1, 22, 5, "└────────────────────────────────────────┘")

                Console.CursorVisible = True
                While True
                    Console.BackgroundColor = ConsoleColor.Green
                    Console.ForegroundColor = ConsoleColor.Black
                    Console.SetCursorPosition(8, 11)
                    Player.Name = Console.ReadLine()
                    If Player.Name = "" Then
                        Console.SetCursorPosition(1, 11)
                        Console.ForegroundColor = ConsoleColor.Green
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.Write("│Name:                                   │")
                    Else
                        Console.ForegroundColor = ConsoleColor.Green
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.SetCursorPosition(8, 11)
                        Console.Write(Player.Name)
                        Exit While
                    End If
                End While

                RoleSelection = 4

                While True
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.ForegroundColor = ConsoleColor.Green
                    Console.SetCursorPosition(14, 13)
                    Console.Write("                    ")
                    Console.SetCursorPosition(14, 13)
                    Console.SetCursorPosition(3, 16)
                    Console.Write("                                       ")
                    Console.SetCursorPosition(3, 17)
                    Console.Write("                                       ")
                    Console.SetCursorPosition(3, 18)
                    Console.Write("                                       ")
                    Console.SetCursorPosition(3, 19)
                    Console.Write("                                       ")
                    Console.SetCursorPosition(3, 20)
                    Console.Write("                                       ")
                    Console.SetCursorPosition(3, 21)
                    Console.Write("                                       ")
                    Select Case RoleSelection
                        Case 4
                            Player.Role = "Colonist"
                            Player.Health = 10
                            Player.Strength = 5
                            Player.Toughness = 5
                            Player.MeleeWeapons = 5
                            Player.RangedWeapons = 5
                            'Player.Technology = 5
                            Player.Medicine = 5
                            Player.Bartering = 5
                            Console.SetCursorPosition(3, 16)
                            Console.Write("Colonists are, naturally, the backbone")
                            Console.SetCursorPosition(3, 17)
                            Console.Write("of colonisation efforts. Jack of all ")
                            Console.SetCursorPosition(3, 18)
                            Console.Write("trades, they excel at nothing but")
                            Console.SetCursorPosition(3, 19)
                            Console.Write("posess adequate skills in all areas.")
                            Console.SetCursorPosition(14, 13)
                        Case 3
                            Player.Role = "Scientist"
                            Player.Health = 10
                            Player.Strength = 3
                            Player.Toughness = 3
                            Player.MeleeWeapons = 4
                            Player.RangedWeapons = 4
                            'Player.Technology = 9
                            Player.Medicine = 9
                            Player.Bartering = 3
                            Console.SetCursorPosition(3, 16)
                            Console.Write("Scientists excel in the fields of ")
                            Console.SetCursorPosition(3, 17)
                            Console.Write("medicine and technology, making them")
                            Console.SetCursorPosition(3, 18)
                            Console.Write("both expert hackers and doctors.")
                            Console.SetCursorPosition(3, 19)
                            Console.Write("However, they lack strength and")
                            Console.SetCursorPosition(3, 20)
                            Console.Write("combat training.")
                            Console.SetCursorPosition(14, 13)
                        Case 2
                            Player.Role = "Soldier"
                            Player.Health = 10
                            Player.Strength = 6
                            Player.Toughness = 6
                            Player.MeleeWeapons = 5
                            Player.RangedWeapons = 9
                            'Player.Technology = 4
                            Player.Medicine = 2
                            Player.Bartering = 3
                            Console.SetCursorPosition(3, 16)
                            Console.Write("Soldiers accompany colonisation efforts")
                            Console.SetCursorPosition(3, 17)
                            Console.Write("to guard settlers from both fauna and")
                            Console.SetCursorPosition(3, 18)
                            Console.Write("internal revolt. They posess great")
                            Console.SetCursorPosition(3, 19)
                            Console.Write("skill with ranged weapons, though their")
                            Console.SetCursorPosition(3, 20)
                            Console.Write("knowledge of medicine and technology is")
                            Console.SetCursorPosition(3, 21)
                            Console.Write("lacking.")
                            Console.SetCursorPosition(14, 13)
                        Case 1
                            Player.Role = "Farmer"
                            Player.Health = 10
                            Player.Strength = 6
                            Player.Toughness = 6
                            Player.MeleeWeapons = 9
                            Player.RangedWeapons = 2
                            'Player.Technology = 2
                            Player.Medicine = 6
                            Player.Bartering = 4
                            Console.SetCursorPosition(3, 16)
                            Console.Write("Farmers are a vital part of a colony,")
                            Console.SetCursorPosition(3, 17)
                            Console.Write("providing food and small medical aid. ")
                            Console.SetCursorPosition(3, 18)
                            Console.Write("While their skill with firearms and")
                            Console.SetCursorPosition(3, 19)
                            Console.Write("technology leaves something to be ")
                            Console.SetCursorPosition(3, 20)
                            Console.Write("desired, their hand to hand ability")
                            Console.SetCursorPosition(3, 21)
                            Console.Write("is second to none.")
                            Console.SetCursorPosition(14, 13)
                        Case 0
                            Player.Role = "Merchant"
                            Player.Health = 10
                            Player.Strength = 4
                            Player.Toughness = 4
                            Player.MeleeWeapons = 4
                            Player.RangedWeapons = 6
                            'Player.Technology = 5
                            Player.Medicine = 3
                            Player.Bartering = 9
                            Console.SetCursorPosition(3, 16)
                            Console.Write("Merchants hold the power of the economy")
                            Console.SetCursorPosition(3, 17)
                            Console.Write("in their educated hands. They may not ")
                            Console.SetCursorPosition(3, 18)
                            Console.Write("be too skilled in close quarters, but")
                            Console.SetCursorPosition(3, 19)
                            Console.Write("their silver tongues grant them cheaper")
                            Console.SetCursorPosition(3, 20)
                            Console.Write("prices.")
                            Console.SetCursorPosition(14, 13)
                    End Select
                    Console.BackgroundColor = ConsoleColor.Green
                    Console.ForegroundColor = ConsoleColor.Black
                    Console.Write(Player.Role)
                    Choice = Console.ReadKey(True).Key
                    Select Case Choice
                        Case 38
                            If RoleSelection < 4 Then
                                RoleSelection += 1
                            End If
                        Case 40
                            If RoleSelection > 0 Then
                                RoleSelection -= 1
                            End If
                        Case 13
                            Console.BackgroundColor = ConsoleColor.Black
                            Console.ForegroundColor = ConsoleColor.Green
                            Console.SetCursorPosition(14, 13)
                            Console.Write("                    ")
                            Console.SetCursorPosition(14, 13)
                            Console.Write(Player.Role)
                            Exit While
                    End Select
                End While

                System.Threading.Thread.Sleep(500)


                Console.ReadLine()

            Case 66         'b
                ConsoleOutput.SlowWrite(1, 4, 1, "                      ")
                ConsoleOutput.SlowWrite(1, 6, 1, "                                        ")
                ConsoleOutput.SlowWrite(1, 7, 1, "               ")
                ConsoleOutput.SlowWrite(1, 8, 1, "                                    ")

                ConsoleOutput.SlowWrite(1, 4, 5, "Help Centre")
                ConsoleOutput.SlowWrite(1, 6, 5, "Following long periods of cryogenic hibernation, temporary memory loss")
                ConsoleOutput.SlowWrite(1, 7, 5, "is a common but harmless occurence. To answer any questions you may have,")
                ConsoleOutput.SlowWrite(1, 8, 5, "Tacton has set up a repository to help ease you back into the real world.")
                ConsoleOutput.SlowWrite(1, 11, 5, "A - Where am I?")
                ConsoleOutput.SlowWrite(1, 12, 5, "B - What day is it?")
                ConsoleOutput.SlowWrite(1, 13, 5, "C - Is any of this real?")

                InnerChoice = Console.ReadKey(True).Key

                Select Case InnerChoice

                    Case 65     'a

                        ConsoleOutput.SlowWrite(1, 4, 1, "           ")
                        ConsoleOutput.SlowWrite(1, 6, 1, "                                                                      ")
                        ConsoleOutput.SlowWrite(1, 7, 1, "                                                                         ")
                        ConsoleOutput.SlowWrite(1, 8, 1, "                                                                         ")
                        ConsoleOutput.SlowWrite(1, 11, 1, "               ")
                        ConsoleOutput.SlowWrite(1, 12, 1, "                   ")
                        ConsoleOutput.SlowWrite(1, 13, 1, "                          ")

                        ConsoleOutput.SlowWrite(1, 4, 5, "Your Location")
                        ConsoleOutput.SlowWrite(1, 6, 5, "The most common question asked by individuals leaving cryogenic hibernation is,")
                        ConsoleOutput.SlowWrite(1, 7, 5, "naturally, 'where am I?'. You have spent the past [173.2] years in a state of")
                        ConsoleOutput.SlowWrite(1, 8, 5, "suspended animation, your body effectively frozen and your brain connected to")
                        ConsoleOutput.SlowWrite(1, 9, 5, "our state of the art life support and neural network mainframe. During this")
                        ConsoleOutput.SlowWrite(1, 10, 5, "period of time, you would have experienced a hyper realistic virtual reality")
                        ConsoleOutput.SlowWrite(1, 11, 5, "emulating earth almost perfectly. Our records indicate you have lived ")
                        ConsoleOutput.SlowWrite(1, 12, 5, "approximately [5] lives during this time. You are currently aboard the colony")
                        ConsoleOutput.SlowWrite(1, 13, 5, "ship 'Hague', bound for the Dubb cluster. The ship 'Henson's Gambit' should")
                        ConsoleOutput.SlowWrite(1, 14, 5, "have already arrived there 16 years ago and established an initial colony; ")
                        ConsoleOutput.SlowWrite(1, 16, 5, "you and the other 8391 crew are to assist in the colony's development and ")
                        ConsoleOutput.SlowWrite(1, 17, 5, "bolster the existing population.")
                        ConsoleOutput.SlowWrite(1, 19, 5, "Estimated arrival time:      -23.2 Years")




                        Console.ReadLine()
                    Case 66     'b

                    Case 67     'c

                End Select
            Case 67         'c

        End Select
    End Sub

    Sub ClearEntityMap()
        For X = 0 To 63
            For Y = 0 To 39
                Entitymap(X, Y) = 0
            Next
        Next
    End Sub

    Sub Overworld()
        Dim Key As Integer
        Dim ChoiceMade As Boolean = False
        Dim OldOverworldX As Integer
        Dim OldOverworldY As Integer
        Console.Clear()
        ConsoleOutput.RenderWorldMap()
        ConsoleOutput.RenderOverworldGUI()
        OldOverworldX = OverworldX
        OldOverworldY = OverworldY
        While True
            ChoiceMade = False
            Console.SetCursorPosition(OldOverworldX, OldOverworldY)
            Console.BackgroundColor = TerrainLookupTable(1, WorldMap(OldOverworldX, OldOverworldY))
            Console.ForegroundColor = TerrainLookupTable(0, WorldMap(OldOverworldX, OldOverworldY))
            Console.Write(TerrainLookupTable(2, WorldMap(OldOverworldX, OldOverworldY)))
            Console.SetCursorPosition(OverworldX, OverworldY)
            Console.BackgroundColor = TerrainLookupTable(1, WorldMap(OverworldX, OverworldY))
            Console.ForegroundColor = ConsoleColor.White
            Console.Write("☺")
            While ChoiceMade = False
                ConsoleOutput.RenderOverworldGUI()
                Player.UpdateHunger()
                Key = Console.ReadKey(True).Key
                OldOverworldX = OverworldX
                OldOverworldY = OverworldY
                Select Case Key

                    Case 37 'left arrow
                        If OverworldX > 0 Then
                            'If WorldMap(OverworldX - 1, OverworldY) <> 7 Then
                            OverworldX -= 1
                            Player.TimeAlive += 360
                            'End If
                        End If
                        ChoiceMade = True
                    Case 38 'up arrow
                        If OverworldY > 0 Then
                            'If WorldMap(OverworldX, OverworldY - 1) <> 7 Then
                            OverworldY -= 1
                            Player.TimeAlive += 360
                            'End If
                        End If
                        ChoiceMade = True
                    Case 39 'right arrow
                        If OverworldX < WorldWidth Then
                            'If WorldMap(OverworldX + 1, OverworldY) <> 7 Then
                            OverworldX += 1
                            Player.TimeAlive += 360
                            'End If
                        End If
                        ChoiceMade = True
                    Case 40 'down arrow
                        If OverworldY < WorldHeight Then
                            'If WorldMap(OverworldX, OverworldY + 1) <> 7 Then
                            OverworldY += 1
                            Player.TimeAlive += 360
                            'End If
                        End If
                        ChoiceMade = True
                    Case 27 'esc key
                        Call InGameOptionsMenu()
                    Case 73 'i key
                        Call Player.ViewInventory()
                        ConsoleOutput.RenderOverworldGUI()
                        ConsoleOutput.RenderWorldMap()
                End Select
            End While

            If WorldMap(OverworldX, OverworldY) = 26 Then
                Call DungeonOverview(OverworldX, OverworldY)
            End If

            If WorldMap(OverworldX, OverworldY) = 27 Then
                Call TownOverview()
            End If

        End While

    End Sub

    Sub GameLoop(ByVal DungeonID As Integer)
        Dim Turn As Integer = 1                         'the current 'turn'.
        Dim LevelID As Integer = 0
        Dim ExitLoop As Boolean
        Dim DungeonString As String
        Dim DungeonInfo() As String

        Player.NewlyEnteredDungeon = True

        Call LoadMap(DungeonID, LevelID)

        Player.PosX = UpStair(0)
        Player.PosY = UpStair(1)

        Call ConsoleOutput.RenderMap()
        Call LineOfSight(Player.PosX, Player.PosY)
        Call ConsoleOutput.RenderMap()
        Call ConsoleOutput.RenderGUI()

        Call ClearEntityMap()

        Console.SetCursorPosition(Player.PosX, Player.PosY)
        Console.BackgroundColor = ConsoleColor.Cyan
        Console.ForegroundColor = ConsoleColor.White
        Console.Write("☺")
        Entitymap(Player.PosX, Player.PosY) = 1                                            'Marks the player on the entity map.

        ExitLoop = False

        Call GenerateEnemies()
        Player.Initialise()

        ConsoleOutput.RenderEnemies()
        CurrentFloor = 0

        DungeonString = GetDungeonInfo(DungeonID)
        DungeonInfo = DungeonString.Split(";")
        MaxEnemyLevel = 12 / ((CurrentFloor + 1) / (DungeonInfo(2) + 1))

        While ExitLoop = False
            Player.TimeAlive += 1
            Player.Move()
            Player.NewlyEnteredDungeon = False
            Player.PlayerCheckSquare(LevelID, DungeonID)
            For X = 0 To EnemyNumber
                If Enemy(X).Enabled = True Then
                    Enemy(X).Move()
                End If
            Next
            ConsoleOutput.RenderEnemies()
        End While

    End Sub

    Sub GenerateEnemies()
        Dim Rand As Integer
        Dim X As Integer
        Dim Y As Integer
        Dim EnemyType As Integer

        Rand = CInt((5 * Rnd()) + 1)

        EnemyNumber = Rand
        ReDim Enemy(Rand)

        For X = 0 To EnemyNumber
            Enemy(X) = New Entity
            If CurrentFloor < 12 Then
                EnemyType = CInt(Player.Level * Rnd() + 1)
            Else
                EnemyType = 12
            End If
            'EnemyType = CInt((MaxEnemyLevel - (MaxEnemyLevel / 2) * Rnd()) + (MaxEnemyLevel / 2))
            Enemy(X).Initialise(EnemyType)
            Enemy(X).Enabled = True
            Enemy(X).EntityMapID = (X + 2)
        Next

    End Sub

    Sub LineOfSight(ByVal PosX As Integer, ByVal PosY As Integer)
        Dim X As Double
        Dim Y As Double
        Dim RayCount As Double

        If SolidMap(PosX, PosY) = 2 Then
            SolidMap(PosX, PosY) = 0
        End If

        For TempX As Integer = 0 To 63
            For TempY As Integer = 0 To 39
                If VisibilityMap(TempX, TempY) = 2 Then
                    VisibilityMap(TempX, TempY) = 1
                    SaveVisibilityMap(TempX, TempY) = 1
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
            SaveVisibilityMap(CInt(RayPosX), CInt(RayPosY)) = 2
            If SolidMap(CInt(RayPosX), CInt(RayPosY)) = 1 Then
                Return
            ElseIf SolidMap(CInt(RayPosX), CInt(RayPosY)) = 2 Then
                Return
            End If

            RayPosX += X
            RayPosY += Y
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
                OutputString = ("Here lies " & Description)
                ReDim Preserve EventLog(EventlogSize + 1)
                EventlogSize += 1
                EventLog(EventlogSize) = OutputString
                ConsoleOutput.RenderEventLog()
                Return
            End If
        End If
    End Sub

    Sub InGameOptionsMenu()
        Dim Choice As Integer
        Dim TimeString As String
        Dim DateString As String
        Console.Clear()

        TimeString = System.DateTime.UtcNow

        While True
            Console.SetCursorPosition(0, 0)
            Console.Write("Menu")
            Console.SetCursorPosition(8, 0)
            Console.Write(TimeString)
        End While

    End Sub

    Sub DungeonOverview(ByVal PosX As Integer, ByVal PosY As Integer)
        Dim DungeonString As String
        Dim DungeonData() As String
        Dim DungeonID As Integer
        Dim Choice As Integer
        For C = 0 To 3
            DungeonString = GetDungeonInfo(C)
            DungeonData = DungeonString.Split(";")
            If DungeonData(3) = PosX Then
                If DungeonData(4) = PosY Then
                    DungeonID = C
                    Exit For
                End If
            End If
        Next
        DungeonData(0) = DungeonData(0)
        Console.ForegroundColor = ConsoleColor.White
        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()
        ConsoleOutput.RenderGUI()
        Console.SetCursorPosition(1, 1)
        Console.Write("You are come to " & DungeonData(0))
        Console.SetCursorPosition(1, 3)
        Console.Write("If records serve correctly, it should extend some " & DungeonData(2))
        Console.SetCursorPosition(1, 4)
        Console.Write("floors into the earth.")
        Console.SetCursorPosition(1, 5)
        'Console.Write("From the local tales, you could face monsters up to level " & (DungeonData(1) * DungeonData(2)) & ".")
        Select Case DungeonID
            Case 0
                If Player.Fort1Cleared = True Then
                    Console.SetCursorPosition(1, 7)
                    Console.Write("You have already vanquished this fortress!")
                    Console.ReadLine()
                    Call Overworld()
                End If
            Case 1
                If Player.Fort2Cleared = True Then
                    Console.SetCursorPosition(1, 7)
                    Console.Write("You have already vanquished this fortress!")
                    Console.ReadLine()
                    Call Overworld()
                End If
            Case 2
                If Player.Fort3Cleared = True Then
                    Console.SetCursorPosition(1, 7)
                    Console.Write("You have already vanquished this fortress!")
                    Console.ReadLine()
                    Call Overworld()
                End If
            Case 3
                If Player.Fort4Cleared = True Then
                    Console.SetCursorPosition(1, 7)
                    Console.Write("You have already vanquished this fortress!")
                    Console.ReadLine()
                    Call Overworld()
                End If
        End Select
        Console.SetCursorPosition(1, 7)
        Console.Write("Dare you venture forth, " & Player.Name & "? (Y/N)")
        Choice = Console.ReadKey(True).Key
        Select Case Choice
            Case 89 'Y
                Console.Clear()
                Call GameLoop(DungeonID)
            Case Else
                Call Overworld()
        End Select


    End Sub

    Sub TownOverview()

    End Sub

    Sub GameWon()
        Console.WriteLine("Congratulations! You have vanquished the four dark fortresses of Eudai!")
        Console.ReadLine()
        Call DeleteAllPlayerData()
        Call MainMenu()
    End Sub

#End Region

#Region "Redundant and/or Prototype Algorithms"

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

    Sub MapGenTest()
        Dim RoomNum As Integer                      'The number of rooms to be generated
        Dim CorridorNum As Integer
        Dim InitialRoomNum As Integer               'This variable copies the value of RoomNum before operations are performed on the RoomNum variable
        Dim CurrentRoomCenterX As Integer           'The X position of the centre of the current room being generated
        Dim CurrentRoomCenterY As Integer           'The Y position of the centre of the current room being generated
        Dim RoomWidth As Integer                    'The width of the current room being generated
        Dim RoomHeight As Integer                   'The height of the current room being generated
        Dim PosX As Integer                         'The current X position in the map being manipulated
        Dim PosY As Integer                         'The current Y position in the map being manipulated
        Dim OldPosX As Integer
        Dim OldPosY As Integer
        Dim Rand As Integer
        Dim BreakCount As Integer
        Dim DoorX As Integer
        Dim DoorY As Integer
        Dim CorridorLength As Integer
        Dim CurrentDirectionLength As Integer
        Dim CurrentDirection As Integer

        Console.BackgroundColor = ConsoleColor.Black
        Console.Clear()

        Dim DestX As Integer
        Dim DestY As Integer

        For count1 As Integer = 0 To 63
            For count2 As Integer = 0 To 39
                SolidMap(count1, count2) = 1    'Initialises the collision map as being totally solid.
            Next
        Next

        For count1 As Integer = 0 To 63
            For count2 As Integer = 0 To 39
                TerrainMap(count1, count2) = 0  'Initialises the terrain map as being totally blank
            Next
        Next

        RoomNum = (CInt(((10) * Rnd())) + 4)    'decides on a random number of rooms, between 14 and 4
        InitialRoomNum = RoomNum
        CorridorNum = (CInt(((6 - 2) * Rnd())) + 2)

        ReDim Preserve RoomCentres(1, RoomNum - 1)

        While True
            CurrentRoomCenterX = CInt((62 - 2) * Rnd())
            CurrentRoomCenterY = CInt((38 - 2) * Rnd())
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

                            Exit While

                        End If
                    End If
                End If
            End If
        End While

        While RoomNum <> 0

            Rand = CInt(Rnd())

            If Rand = 0 Then    'Generate a room

                PosX = CInt((62 - 2) * Rnd())   'get a random x position
                PosY = CInt((38 - 2) * Rnd())   'get a random y position
                While SolidMap(PosX, PosY) = 1
                    PosX = CInt((62 - 2) * Rnd())
                    PosY = CInt((38 - 2) * Rnd())
                End While
                Rand = CInt(2 * Rnd())  'decide the direction in which to look for a wall (between 0 and 3)
                Select Case Rand
                    Case 0
                        While SolidMap(PosX, PosY) = 0
                            PosX += 1
                        End While
                    Case 1
                        While SolidMap(PosX, PosY) = 0
                            PosY -= 1
                        End While
                    Case 2
                        While SolidMap(PosX, PosY) = 0
                            PosX -= 1
                        End While
                    Case 3
                        While SolidMap(PosX, PosY) = 0
                            PosY += 1
                        End While
                End Select

                DoorX = PosX
                DoorY = PosY

                RoomWidth = CInt((8 - 4) * Rnd()) + 3
                RoomHeight = CInt((8 - 4) * Rnd()) + 3

                If Rand = 0 Then
                    PosX = PosX + (CInt(RoomWidth / 2) + 1)
                ElseIf Rand = 1 Then
                    PosY = PosY - (CInt(RoomHeight / 2) + 1)
                ElseIf Rand = 2 Then
                    PosX = PosX - (CInt(RoomWidth / 2) + 1)
                ElseIf Rand = 3 Then
                    PosY = PosY + (CInt(RoomHeight / 2) + 1)
                End If
                CurrentRoomCenterX = PosX
                CurrentRoomCenterY = PosY

                PosX = CurrentRoomCenterX - CInt(RoomWidth / 2)
                PosY = CurrentRoomCenterY - CInt(RoomHeight / 2)

                If CurrentRoomCenterX + CInt(RoomWidth / 2) < 62 Then                   'Checks that the room will not be out of bounds by clipping the 'Right' of the map
                    If CurrentRoomCenterX - CInt(RoomWidth / 2) > 1 Then                'Checks that the room will not be out of bounds by clipping the 'Left' of the map
                        If CurrentRoomCenterY + CInt(RoomHeight / 2) < 38 Then          'Checks that the room will not be out of bounds by clipping the 'Bottom' of the map  
                            If CurrentRoomCenterY - CInt(RoomHeight / 2) > 1 Then       'Checks that the room will not be out of bounds by clipping the 'Top' of the map


                                For Count4 As Integer = 0 To RoomHeight                                     'While the room has not had all vertical lines drawn
                                    For Count3 As Integer = 0 To RoomWidth                                  'For 0 to the desired length of the room
                                        SolidMap(PosX, PosY) = 0
                                        PosX += 1
                                    Next
                                    PosX = CurrentRoomCenterX - CInt(RoomWidth / 2)
                                    PosY += 1
                                Next

                                RoomCentres(0, RoomNum - 1) = CurrentRoomCenterX
                                RoomCentres(1, RoomNum - 1) = CurrentRoomCenterY
                                RoomNum -= 1

                                PosX = DoorX
                                PosY = DoorY

                                While PosX <> CurrentRoomCenterX
                                    While PosY <> CurrentRoomCenterY
                                        SolidMap(PosX, PosY) = 0
                                        If PosY < CurrentRoomCenterY Then
                                            PosY += 1
                                        Else
                                            PosY -= 1
                                        End If
                                    End While
                                    SolidMap(PosX, PosY) = 0
                                    If PosX < CurrentRoomCenterX Then
                                        PosX += 1
                                    Else
                                        PosX -= 1
                                    End If
                                End While
                                TerrainMap(PosX, PosY) = 3                  'Door in terrainmap
                                SolidMap(PosX, PosY) = 2                    'Door in solidmap
                                TerrainMap(DoorX, DoorY) = 3                'Door in terrainmap
                                SolidMap(DoorX, DoorY) = 2                  'Door in solidmap
                            End If
                        End If
                    End If
                End If

                Rand = CInt(10 * Rnd())

            Else    'Generate a Corridor

                If CorridorNum > 0 Then

                    CorridorLength = CInt((18 * Rnd()) - 2)

                    PosX = CInt((62 - 2) * Rnd())   'get a random x position
                    PosY = CInt((38 - 2) * Rnd())   'get a random y position
                    While SolidMap(PosX, PosY) = 1
                        PosX = CInt((62 - 2) * Rnd())
                        PosY = CInt((38 - 2) * Rnd())
                    End While
                    Rand = CInt(2 * Rnd())  'decide the direction in which to look for a wall (between 0 and 3)
                    Select Case Rand
                        Case 0
                            While SolidMap(PosX, PosY) = 0
                                PosX += 1
                            End While
                        Case 1
                            While SolidMap(PosX, PosY) = 0
                                PosY -= 1
                            End While
                        Case 2
                            While SolidMap(PosX, PosY) = 0
                                PosX -= 1
                            End While
                        Case 3
                            While SolidMap(PosX, PosY) = 0
                                PosY += 1
                            End While
                    End Select

                    DoorX = PosX
                    DoorY = PosY

                    OldPosX = PosX
                    OldPosY = PosY

                    While CorridorLength <> 0
                        CurrentDirection = CInt(3 * Rnd())
                        CurrentDirectionLength = CInt(((CorridorLength - 1) * Rnd()) + 1)

                        For Count = 0 To CurrentDirectionLength
                            OldPosX = PosX
                            OldPosY = PosY
                            Select Case CurrentDirection
                                Case 0
                                    If PosX + 1 < 62 Then
                                        PosX += 1
                                        CorridorLength -= 1
                                    Else
                                        Exit For
                                    End If
                                Case 1
                                    If PosY + 1 < 38 Then
                                        PosY += 1
                                        CorridorLength -= 1
                                    Else
                                        Exit For
                                    End If
                                Case 2
                                    If PosX - 1 > 1 Then
                                        PosX -= 1
                                        CorridorLength -= 1
                                    Else
                                        Exit For
                                    End If
                                Case 3
                                    If PosY - 1 > 1 Then
                                        PosY -= 1
                                        CorridorLength -= 1
                                    Else
                                        Exit For
                                    End If
                            End Select

                            If SolidMap(PosX, PosY) = 0 Then
                                If SolidMap(OldPosX, OldPosY) = 1 Then
                                    SolidMap(OldPosX, OldPosY) = 2
                                    TerrainMap(OldPosX, OldPosY) = 3
                                    Exit While
                                End If
                            End If

                            SolidMap(PosX, PosY) = 0

                        Next
                    End While

                    CorridorNum -= 1

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
                    Call TerrainCheck(PosX, PosY)
                End If
            Next
        Next

        Rand = CInt((InitialRoomNum - 1) * Rnd())

        PosX = RoomCentres(0, Rand)
        PosY = RoomCentres(1, Rand)

        TerrainMap(PosX, PosY) = 5                     'Makes the area an upwards staircase

        UpStair(0) = PosX
        UpStair(1) = PosY

        While PosX = UpStair(0) And PosY = UpStair(1)
            Rand = CInt((InitialRoomNum - 1) * Rnd())


            PosX = RoomCentres(0, Rand)
            PosY = RoomCentres(1, Rand)
        End While

        TerrainMap(PosX, PosY) = 6                     'Makes the area a downwards staircase

        DownStair(0) = PosX
        DownStair(1) = PosY

        For CountY As Integer = 0 To 39
            For CountX As Integer = 0 To 63
                PosX = CountX
                PosY = CountY
                If TerrainMap(PosX, PosY) = 3 Then
                    Continue For
                ElseIf TerrainMap(PosX, PosY) = 5 Then
                    Continue For
                ElseIf TerrainMap(PosX, PosY) = 6 Then
                    Continue For
                ElseIf SolidMap(PosX, PosY) = 0 Then
                    TerrainMap(PosX, PosY) = 1           'Makes the area a normal floor tile (1)
                Else
                    Call TerrainCheck(PosX, PosY)
                End If
            Next
        Next

        For X As Integer = 0 To 63
            For Y As Integer = 0 To 39
                VisibilityMap(X, Y) = 2     'FOR TESTING PURPOSES. WHEN IMPLEMENTING CHANGE 2 TO 0!!!
            Next
        Next

        'Call GameLoop()
        Return
    End Sub

    Sub SquareCompare(ByVal X As Integer, ByVal Y As Integer, ByRef MoveMade As Boolean)
        If FloodMap(X, Y) = 0 Then
            Return
        ElseIf FloodMap(X, Y) = 1 Then
            Try
                If FloodMap(X + 1, Y) = 0 Then
                    FloodMap(X + 1, Y) = 1
                    MoveMade = True
                End If
            Catch ex As Exception
            End Try

            Try
                If FloodMap(X, Y - 1) = 0 Then
                    FloodMap(X, Y - 1) = 1
                    MoveMade = True
                End If
            Catch ex As Exception
            End Try

            Try
                If FloodMap(X - 1, Y) = 0 Then
                    FloodMap(X - 1, Y) = 1
                    MoveMade = True
                End If
            Catch ex As Exception
            End Try

            Try
                If FloodMap(X, Y + 1) = 0 Then
                    FloodMap(X, Y + 1) = 1
                    MoveMade = True
                End If
            Catch ex As Exception
            End Try

        End If
    End Sub

    Sub TestFloodFillRender()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White
        For X = 0 To 63
            For Y = 0 To 39
                Console.SetCursorPosition(X, Y)
                Select Case FloodMap(X, Y)
                    Case 0
                        Console.Write(".")
                    Case 1
                        Console.Write("*")
                    Case 2
                        Console.Write("#")
                End Select
            Next
        Next
    End Sub

    Sub CorridorGenerationOld(ByVal CurrentRoomCenterX As Integer, ByVal CurrentRoomCenterY As Integer, ByVal PosX As Integer, ByVal PosY As Integer, ByVal DestX As Integer, ByVal DestY As Integer, ByVal RoomNum As Integer)


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

            While (PosY <> DestY)

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

            While (PosX <> DestX)
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

        While (PosY <> DestY)
            If PosY < DestY Then
                PosY += 1
            ElseIf PosY > DestY Then
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

        While (PosX <> DestX)
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

    Sub OldNewGame()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.Green
        Console.Clear()
        Console.CursorVisible = True
        ConsoleOutput.SlowWrite(0, 0, 100, "Booting Tacton Systems Neural Interface V2.94.")
        For C = 0 To 3
            System.Threading.Thread.Sleep(500)
            Console.Write(".")
        Next
        ConsoleOutput.SlowWrite(0, 1, 100, "Asessing Vitals.")
        For C = 0 To 3
            System.Threading.Thread.Sleep(500)
            Console.Write(".")
        Next
        ConsoleOutput.SlowWrite(0, 2, 100, "Synchronizing Brain Patterns.")
        For C = 0 To 3
            System.Threading.Thread.Sleep(500)
            Console.Write(".")
        Next
        ConsoleOutput.SlowWrite(0, 3, 100, "Connecting to Tacton Internal Network.")
        For C = 0 To 3
            System.Threading.Thread.Sleep(500)
            Console.Write(".")
        Next
        ConsoleOutput.SlowWrite(0, 4, 100, "Loading User Interface.")
        For C = 0 To 3
            System.Threading.Thread.Sleep(500)
            Console.Write(".")
        Next
        Call NewGameMenu()
    End Sub

    Sub OldNewGameMenu()
        Dim Choice As Integer
        Dim InnerChoice As Integer
        Dim ChoiceMade As Boolean = False
        Dim RoleSelection As Integer
        Console.Clear()
        Console.SetCursorPosition(0, 0)
        Console.WriteLine("╒════════════════════════════════════════════════════════════════════════════════════════╕")
        For Y As Integer = 1 To 58
            Console.SetCursorPosition(0, Y)
            Console.Write("│")
            Console.SetCursorPosition(89, Y)
            Console.Write("│")
            System.Threading.Thread.Sleep(5)
        Next
        Console.SetCursorPosition(0, 59)
        Console.Write("╘════════════════════════════════════════════════════════════════════════════════════════╛")
        Console.SetCursorPosition(0, 0)
        ConsoleOutput.SlowWrite(1, 1, 5, "Tacton Systems Neural Interface V2.94")
        ConsoleOutput.SlowWrite(1, 4, 5, "Neural Hub Main Menu:")
        ConsoleOutput.SlowWrite(1, 6, 5, "A - Commence Cryostasis Revival Process")
        ConsoleOutput.SlowWrite(1, 7, 5, "B - Help Centre")
        ConsoleOutput.SlowWrite(1, 8, 5, "C - Return to Cryogenic Hibernation")
        ConsoleOutput.SlowWrite(70, 45, 5, " █████  █████ ")
        ConsoleOutput.SlowWrite(70, 46, 5, "██████████████")
        ConsoleOutput.SlowWrite(70, 47, 5, "██   ▒██▒   ██")
        ConsoleOutput.SlowWrite(70, 48, 5, "    ▒▒██▒▒    ")
        ConsoleOutput.SlowWrite(70, 49, 5, "   ▒▒ ██ ▒▒   ")
        ConsoleOutput.SlowWrite(70, 50, 5, "  ▒▒▒▒██▒▒▒▒")
        ConsoleOutput.SlowWrite(70, 51, 5, " ▒▒   ██   ▒▒ ")
        ConsoleOutput.SlowWrite(70, 52, 5, "▒▒    ██    ▒▒")
        ConsoleOutput.SlowWrite(70, 53, 5, "     ████     ")
        ConsoleOutput.SlowWrite(70, 54, 5, "  ██████████  ")
        ConsoleOutput.SlowWrite(70, 55, 5, "  ████  ████  ")
        ConsoleOutput.SlowWrite(74, 57, 5, "TACTON")
        Console.CursorVisible = False
        Console.SetCursorPosition(0, 0)

        Choice = Console.ReadKey(True).Key

        Select Case Choice
            Case 65         'a
                ConsoleOutput.SlowWrite(1, 4, 1, "                      ")
                ConsoleOutput.SlowWrite(1, 6, 1, "                                        ")
                ConsoleOutput.SlowWrite(1, 7, 1, "               ")
                ConsoleOutput.SlowWrite(1, 8, 1, "                                    ")
                ConsoleOutput.SlowWrite(1, 4, 5, "Missing or Corrupted Records")
                ConsoleOutput.SlowWrite(1, 6, 5, "Due to unforseen circumstances, Tacton has missing data regarding your personal")
                ConsoleOutput.SlowWrite(1, 7, 5, "information, such as your name and occupation. It is necessary for you to provide")
                ConsoleOutput.SlowWrite(1, 8, 5, "this data before commencing the cryostasis revival process.")
                ConsoleOutput.SlowWrite(1, 10, 5, "┌────────────────────────────────────────┐")
                ConsoleOutput.SlowWrite(1, 11, 5, "│Name:")
                ConsoleOutput.SlowWrite(42, 11, 5, "│")
                ConsoleOutput.SlowWrite(1, 12, 5, "├────────────────────────────────────────┤")
                ConsoleOutput.SlowWrite(1, 13, 5, "│Occupation:")
                ConsoleOutput.SlowWrite(42, 13, 5, "│")
                ConsoleOutput.SlowWrite(1, 14, 5, "├────────────────────────────────────────┤")
                ConsoleOutput.SlowWrite(1, 15, 5, "│Job Description:")
                ConsoleOutput.SlowWrite(42, 15, 5, "│")
                ConsoleOutput.SlowWrite(1, 16, 5, "│")
                ConsoleOutput.SlowWrite(42, 16, 5, "│")
                ConsoleOutput.SlowWrite(1, 17, 5, "│")
                ConsoleOutput.SlowWrite(42, 17, 5, "│")
                ConsoleOutput.SlowWrite(1, 18, 5, "│")
                ConsoleOutput.SlowWrite(42, 18, 5, "│")
                ConsoleOutput.SlowWrite(1, 19, 5, "│")
                ConsoleOutput.SlowWrite(42, 19, 5, "│")
                ConsoleOutput.SlowWrite(1, 20, 5, "│")
                ConsoleOutput.SlowWrite(42, 20, 5, "│")
                ConsoleOutput.SlowWrite(1, 21, 5, "│")
                ConsoleOutput.SlowWrite(42, 21, 5, "│")
                ConsoleOutput.SlowWrite(1, 22, 5, "└────────────────────────────────────────┘")

                Console.CursorVisible = True
                While True
                    Console.BackgroundColor = ConsoleColor.Green
                    Console.ForegroundColor = ConsoleColor.Black
                    Console.SetCursorPosition(8, 11)
                    Player.Name = Console.ReadLine()
                    If Player.Name = "" Then
                        Console.SetCursorPosition(1, 11)
                        Console.ForegroundColor = ConsoleColor.Green
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.Write("│Name:                                   │")
                    Else
                        Console.ForegroundColor = ConsoleColor.Green
                        Console.BackgroundColor = ConsoleColor.Black
                        Console.SetCursorPosition(8, 11)
                        Console.Write(Player.Name)
                        Exit While
                    End If
                End While

                RoleSelection = 4

                While True
                    Console.BackgroundColor = ConsoleColor.Black
                    Console.ForegroundColor = ConsoleColor.Green
                    Console.SetCursorPosition(14, 13)
                    Console.Write("                    ")
                    Console.SetCursorPosition(14, 13)
                    Console.SetCursorPosition(3, 16)
                    Console.Write("                                       ")
                    Console.SetCursorPosition(3, 17)
                    Console.Write("                                       ")
                    Console.SetCursorPosition(3, 18)
                    Console.Write("                                       ")
                    Console.SetCursorPosition(3, 19)
                    Console.Write("                                       ")
                    Console.SetCursorPosition(3, 20)
                    Console.Write("                                       ")
                    Console.SetCursorPosition(3, 21)
                    Console.Write("                                       ")
                    Select Case RoleSelection
                        Case 4
                            Player.Role = "Colonist"
                            Player.Health = 10
                            Player.Strength = 5
                            Player.Toughness = 5
                            Player.MeleeWeapons = 5
                            Player.RangedWeapons = 5
                            'Player.Technology = 5
                            Player.Medicine = 5
                            Player.Bartering = 5
                            Console.SetCursorPosition(3, 16)
                            Console.Write("Colonists are, naturally, the backbone")
                            Console.SetCursorPosition(3, 17)
                            Console.Write("of colonisation efforts. Jack of all ")
                            Console.SetCursorPosition(3, 18)
                            Console.Write("trades, they excel at nothing but")
                            Console.SetCursorPosition(3, 19)
                            Console.Write("posess adequate skills in all areas.")
                            Console.SetCursorPosition(14, 13)
                        Case 3
                            Player.Role = "Scientist"
                            Player.Health = 10
                            Player.Strength = 3
                            Player.Toughness = 3
                            Player.MeleeWeapons = 4
                            Player.RangedWeapons = 4
                            'Player.Technology = 9
                            Player.Medicine = 9
                            Player.Bartering = 3
                            Console.SetCursorPosition(3, 16)
                            Console.Write("Scientists excel in the fields of ")
                            Console.SetCursorPosition(3, 17)
                            Console.Write("medicine and technology, making them")
                            Console.SetCursorPosition(3, 18)
                            Console.Write("both expert hackers and doctors.")
                            Console.SetCursorPosition(3, 19)
                            Console.Write("However, they lack strength and")
                            Console.SetCursorPosition(3, 20)
                            Console.Write("combat training.")
                            Console.SetCursorPosition(14, 13)
                        Case 2
                            Player.Role = "Soldier"
                            Player.Health = 10
                            Player.Strength = 6
                            Player.Toughness = 6
                            Player.MeleeWeapons = 5
                            Player.RangedWeapons = 9
                            'Player.Technology = 4
                            Player.Medicine = 2
                            Player.Bartering = 3
                            Console.SetCursorPosition(3, 16)
                            Console.Write("Soldiers accompany colonisation efforts")
                            Console.SetCursorPosition(3, 17)
                            Console.Write("to guard settlers from both fauna and")
                            Console.SetCursorPosition(3, 18)
                            Console.Write("internal revolt. They posess great")
                            Console.SetCursorPosition(3, 19)
                            Console.Write("skill with ranged weapons, though their")
                            Console.SetCursorPosition(3, 20)
                            Console.Write("knowledge of medicine and technology is")
                            Console.SetCursorPosition(3, 21)
                            Console.Write("lacking.")
                            Console.SetCursorPosition(14, 13)
                        Case 1
                            Player.Role = "Farmer"
                            Player.Health = 10
                            Player.Strength = 6
                            Player.Toughness = 6
                            Player.MeleeWeapons = 9
                            Player.RangedWeapons = 2
                            'Player.Technology = 2
                            Player.Medicine = 6
                            Player.Bartering = 4
                            Console.SetCursorPosition(3, 16)
                            Console.Write("Farmers are a vital part of a colony,")
                            Console.SetCursorPosition(3, 17)
                            Console.Write("providing food and small medical aid. ")
                            Console.SetCursorPosition(3, 18)
                            Console.Write("While their skill with firearms and")
                            Console.SetCursorPosition(3, 19)
                            Console.Write("technology leaves something to be ")
                            Console.SetCursorPosition(3, 20)
                            Console.Write("desired, their hand to hand ability")
                            Console.SetCursorPosition(3, 21)
                            Console.Write("is second to none.")
                            Console.SetCursorPosition(14, 13)
                        Case 0
                            Player.Role = "Merchant"
                            Player.Health = 10
                            Player.Strength = 4
                            Player.Toughness = 4
                            Player.MeleeWeapons = 4
                            Player.RangedWeapons = 6
                            'Player.Technology = 5
                            Player.Medicine = 3
                            Player.Bartering = 9
                            Console.SetCursorPosition(3, 16)
                            Console.Write("Merchants hold the power of the economy")
                            Console.SetCursorPosition(3, 17)
                            Console.Write("in their educated hands. They may not ")
                            Console.SetCursorPosition(3, 18)
                            Console.Write("be too skilled in close quarters, but")
                            Console.SetCursorPosition(3, 19)
                            Console.Write("their silver tongues grant them cheaper")
                            Console.SetCursorPosition(3, 20)
                            Console.Write("prices.")
                            Console.SetCursorPosition(14, 13)
                    End Select
                    Console.BackgroundColor = ConsoleColor.Green
                    Console.ForegroundColor = ConsoleColor.Black
                    Console.Write(Player.Role)
                    Choice = Console.ReadKey(True).Key
                    Select Case Choice
                        Case 38
                            If RoleSelection < 4 Then
                                RoleSelection += 1
                            End If
                        Case 40
                            If RoleSelection > 0 Then
                                RoleSelection -= 1
                            End If
                        Case 13
                            Console.BackgroundColor = ConsoleColor.Black
                            Console.ForegroundColor = ConsoleColor.Green
                            Console.SetCursorPosition(14, 13)
                            Console.Write("                    ")
                            Console.SetCursorPosition(14, 13)
                            Console.Write(Player.Role)
                            Exit While
                    End Select
                End While

                System.Threading.Thread.Sleep(500)


                Console.ReadLine()

            Case 66         'b
                ConsoleOutput.SlowWrite(1, 4, 1, "                      ")
                ConsoleOutput.SlowWrite(1, 6, 1, "                                        ")
                ConsoleOutput.SlowWrite(1, 7, 1, "               ")
                ConsoleOutput.SlowWrite(1, 8, 1, "                                    ")

                ConsoleOutput.SlowWrite(1, 4, 5, "Help Centre")
                ConsoleOutput.SlowWrite(1, 6, 5, "Following long periods of cryogenic hibernation, temporary memory loss")
                ConsoleOutput.SlowWrite(1, 7, 5, "is a common but harmless occurence. To answer any questions you may have,")
                ConsoleOutput.SlowWrite(1, 8, 5, "Tacton has set up a repository to help ease you back into the real world.")
                ConsoleOutput.SlowWrite(1, 11, 5, "A - Where am I?")
                ConsoleOutput.SlowWrite(1, 12, 5, "B - What day is it?")
                ConsoleOutput.SlowWrite(1, 13, 5, "C - Is any of this real?")

                InnerChoice = Console.ReadKey(True).Key

                Select Case InnerChoice

                    Case 65     'a

                        ConsoleOutput.SlowWrite(1, 4, 1, "           ")
                        ConsoleOutput.SlowWrite(1, 6, 1, "                                                                      ")
                        ConsoleOutput.SlowWrite(1, 7, 1, "                                                                         ")
                        ConsoleOutput.SlowWrite(1, 8, 1, "                                                                         ")
                        ConsoleOutput.SlowWrite(1, 11, 1, "               ")
                        ConsoleOutput.SlowWrite(1, 12, 1, "                   ")
                        ConsoleOutput.SlowWrite(1, 13, 1, "                          ")

                        ConsoleOutput.SlowWrite(1, 4, 5, "Your Location")
                        ConsoleOutput.SlowWrite(1, 6, 5, "The most common question asked by individuals leaving cryogenic hibernation is,")
                        ConsoleOutput.SlowWrite(1, 7, 5, "naturally, 'where am I?'. You have spent the past [173.2] years in a state of")
                        ConsoleOutput.SlowWrite(1, 8, 5, "suspended animation, your body effectively frozen and your brain connected to")
                        ConsoleOutput.SlowWrite(1, 9, 5, "our state of the art life support and neural network mainframe. During this")
                        ConsoleOutput.SlowWrite(1, 10, 5, "period of time, you would have experienced a hyper realistic virtual reality")
                        ConsoleOutput.SlowWrite(1, 11, 5, "emulating earth almost perfectly. Our records indicate you have lived ")
                        ConsoleOutput.SlowWrite(1, 12, 5, "approximately [5] lives during this time. You are currently aboard the colony")
                        ConsoleOutput.SlowWrite(1, 13, 5, "ship 'Hague', bound for the Dubb cluster. The ship 'Henson's Gambit' should")
                        ConsoleOutput.SlowWrite(1, 14, 5, "have already arrived there 16 years ago and established an initial colony; ")
                        ConsoleOutput.SlowWrite(1, 16, 5, "you and the other 8391 crew are to assist in the colony's development and ")
                        ConsoleOutput.SlowWrite(1, 17, 5, "bolster the existing population.")
                        ConsoleOutput.SlowWrite(1, 19, 5, "Estimated arrival time:      -23.2 Years")




                        Console.ReadLine()
                    Case 66     'b

                    Case 67     'c

                End Select
            Case 67         'c

        End Select
    End Sub

    Sub PlayerCheckSquare(ByRef PosX As Integer, ByRef PosY As Integer, ByRef LevelID As Integer, ByVal DungeonID As Integer, ByRef NewlyEnteredDungeon As Boolean)

        If NewlyEnteredDungeon = False Then
            If PosX = UpStair(0) Then
                If PosY = UpStair(1) Then
                    If LevelID > 0 Then
                        Try
                            Call SaveMap(DungeonID, LevelID)
                            Call LoadMap(DungeonID, LevelID - 1)
                        Catch ex As Exception
                        End Try
                        ClearEntityMap()
                        PosX = DownStair(0)
                        PosY = DownStair(1)
                        Entitymap(PosX, PosY) = 1
                        Console.Clear()
                        ConsoleOutput.RenderMap()
                        Call LineOfSight(PosX, PosY)
                        ConsoleOutput.RenderMap()
                        Call GenerateEnemies()
                        ConsoleOutput.RenderGUI()
                        LevelID -= 1
                        NewlyEnteredDungeon = True
                    End If
                End If
            End If
            If NewlyEnteredDungeon = False Then
                If PosX = DownStair(0) Then
                    If PosY = DownStair(1) Then
                        Try
                            Call SaveMap(DungeonID, LevelID)
                            Call LoadMap(DungeonID, LevelID + 1)
                        Catch ex As Exception
                            Console.Clear()
                            Console.WriteLine("You have reached the bottom floor of the dungeon!")
                            Console.WriteLine(ex)
                            Console.ReadLine()
                            Call TestingMenu()
                        End Try
                        ClearEntityMap()
                        PosX = UpStair(0)
                        PosY = UpStair(1)
                        Entitymap(PosX, PosY) = 1
                        Console.Clear()
                        ConsoleOutput.RenderMap()
                        Call LineOfSight(PosX, PosY)
                        ConsoleOutput.RenderMap()
                        Call GenerateEnemies()
                        ConsoleOutput.RenderGUI()
                        LevelID += 1
                        NewlyEnteredDungeon = True
                    End If
                End If
            End If
        End If


    End Sub

#End Region

End Module
