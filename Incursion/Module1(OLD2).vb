Imports System.IO
Module Module1


    'A map generation algorithm for a roguelike!
    'Written by Samuel Kinnett, 2013


    Dim SolidMap(63, 39) As Integer         'this map consists of 1's and 0's. A 1 indicates solid space e.g. a wall, and a 0 indicates empty space that the player can walk through
    Dim TerrainMap(63, 39) As Integer       'this map consists of a variety of numbers indicating the material of the 'block' in that space, e.g. stone or wood.
    Dim ItemMap(63, 39) As String           'this map contains the location of items in the world. Multiple items in one square are divided by a semicolon, e.g. "WOODEN SWORD:WOODEN SHIELD"
    Dim VisibilityMap(63, 39) As Integer    'this array contains a map showing the currently visible, and previously explored, areas of the map.
    Dim CurrentMapName As String            'this is the name of the current map that will be saved along with the map data. Randomly generated from an array of names.
    Dim MapNameBank(4, 4) As String         'all the potential name segments for a map. Divided into 4 sections.
    Dim RoomCentres(,) As Integer           'stores the centres of each room for the purposes of linking them via corridors.
    Dim UpStair(1) As Integer
    Dim DownStair(1) As Integer

    Sub Main()

        Call LoadData()

    End Sub

    Sub LoadData()

        'Code will go here that checks if the necessary files are in place, and creates them if they aren't. It then loads all the random names into the arrays.
        Console.WriteLine("Press Enter to begin map generation test")
        Console.ReadLine()
        Call MapGen()

    End Sub

    Sub MapGen()

        Dim RoomNum As Integer
        Dim MutateChance As Integer
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

        Call CorridorGenerationTest(CurrentRoomCenterX, CurrentRoomCenterY, PosX, PosY, DestX, DestY, InitialRoomNum)

        For CountY As Integer = 0 To 39
            For CountX As Integer = 0 To 63
                PosX = CountX
                PosY = CountY
                If TerrainMap(PosX, PosY) = 3 Then
                    Continue For
                ElseIf SolidMap(PosX, PosY) = 0 Then
                    TerrainMap(PosX, PosY) = 1           'Makes the area a normal floor tile (1)
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
                        TerrainMap(PosX, PosY) = 3              'Door
                        End If
                End While
                If DestX > PosX Then
                    PosX += 1
                ElseIf DestX < PosX Then
                    PosX -= 1
                End If
                SolidMap(PosX, PosY) = 0
                If TerrainMap(PosX, PosY) = 2 Then
                        TerrainMap(PosX, PosY) = 3              'Door
                    End If
            End While
        Next

    End Sub

    Sub CorridorGenerationTest(ByVal CurrentRoomCenterX As Integer, ByVal CurrentRoomCenterY As Integer, ByVal PosX As Integer, ByVal PosY As Integer, ByVal DestX As Integer, ByVal DestY As Integer, ByVal RoomNum As Integer)


        Dim NumberOfDoors As Integer = 2                                    'Sets the number of doors allowed in the corridor to 2
        Dim MandatoryCorridor As Boolean = False

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

            MandatoryCorridor = False

            If PosX = UpStair(0) Then
                If PosY = UpStair(1) Then
                    If DestX = DownStair(0) Then
                        If DestY = DownStair(1) Then
                            MandatoryCorridor = True
                        End If
                    End If
                End If
            End If

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
                            TerrainMap(PosX, PosY) = 3              'Door
                            NumberOfDoors -= 1
                        Else
                            If MandatoryCorridor = False Then
                                Exit While
                            End If
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
                        TerrainMap(PosX, PosY) = 3              'Door
                        NumberOfDoors -= 1
                    Else
                        If MandatoryCorridor = False Then
                            Exit While
                        End If
                    End If
                End If
            End While
        Next
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
                Call RenderSquare(X, Y)
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
                        Console.BackgroundColor = ConsoleColor.Gray
                        Console.Write("▒")
                    Case 2
                        Console.BackgroundColor = ConsoleColor.DarkGray
                        Console.Write("▒")
                    Case 3
                        Console.BackgroundColor = ConsoleColor.DarkYellow
                        Console.ForegroundColor = ConsoleColor.Yellow
                        Console.Write("▒")
                    Case 10
                        Console.BackgroundColor = ConsoleColor.Cyan
                        Console.ForegroundColor = ConsoleColor.White
                        Console.Write("▒")
                    Case 11
                        Console.BackgroundColor = ConsoleColor.DarkRed
                        Console.ForegroundColor = ConsoleColor.White
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
        Dim Key As Char
        Dim ExitLoop As Boolean
        Dim OldPosX As Integer
        Dim OldPosY As Integer

        PosX = UpStair(0)
        PosY = UpStair(1)

        Call LineOfSight(PosX, PosY)
        Call RenderMap()

        Console.SetCursorPosition(PosX, PosY)
        Console.BackgroundColor = ConsoleColor.Cyan
        Console.ForegroundColor = ConsoleColor.White
        Console.Write("☺")

        ExitLoop = False
        While ExitLoop = False
            OldPosX = PosX
            OldPosY = PosY
            Key = Console.ReadKey(True).KeyChar
            Select Case Key
                Case "w"
                    Try
                        If SolidMap(PosX, PosY - 1) <> 1 Then
                            PosY = PosY - 1
                            Console.SetCursorPosition(PosX, PosY - 1)
                        End If
                    Catch ex As Exception
                        Continue While
                    End Try

                Case "s"
                    Try
                        If SolidMap(PosX, PosY + 1) <> 1 Then
                            PosY = PosY + 1
                            Console.SetCursorPosition(PosX, PosY + 1)
                        End If
                    Catch ex As Exception
                        Continue While
                    End Try
                Case "a"
                    Try
                        If SolidMap(PosX - 1, PosY) <> 1 Then
                            PosX = PosX - 1
                            Console.SetCursorPosition(PosX - 1, PosY)
                        End If
                    Catch ex As Exception
                        Continue While
                    End Try
                Case "d"
                    Try
                        If SolidMap(PosX + 1, PosY) <> 1 Then
                            PosX = PosX + 1
                            Console.SetCursorPosition(PosX + 1, PosY)
                        End If
                    Catch ex As Exception
                        Continue While
                    End Try
                Case "q"
                    ExitLoop = True
            End Select
            Try
                LineOfSight(PosX, PosY)
                RenderSquare(OldPosX, OldPosY)
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
        Dim Ux As Double 'Initial X velocity of the LoS projectile
        Dim Uy As Double 'Initial Y velocity of the LoS projectile
        Dim T As Double  'Time
        Dim Sx As Double 'X displacement
        Dim Sy As Double 'Y displacement
        Dim CalcAngle As Integer
        Dim Force As Integer = 3
        Dim StartPosX As Integer
        Dim StartPosY As Integer
        Dim Collision As Boolean

        StartPosX = PosX
        StartPosY = PosY

        For X As Integer = 0 To 63
            For Y As Integer = 0 To 39
                If VisibilityMap(X, Y) = 2 Then
                    VisibilityMap(X, Y) = 1
                Else
                    VisibilityMap(X, Y) = 0
                End If
            Next
        Next

        For Angle As Integer = 0 To 360 Step 1
            CalcAngle = Angle * 0.0174532925
            Ux = Force * Math.Cos(CalcAngle)    'Works out the intial X velocity using Ux = Initial velocity * Cos(desired angle)
            Uy = Force * Math.Sin(CalcAngle)    'Works out the intial Y velocity using Uy = Initial velocity * Sin(desired angle)
            T = 0                           'Initialises the time variable as 0 seconds.
            Collision = False               'Initialises the Collision variable as false.

            While Collision = False         'While the particle has not hit the ground or an entity

                T += 0.1
                Sx = Ux * T + (0.5 * ((T ^ 2)))
                Sy = Uy * T + (0.5 * ((T ^ 2)))

                StartPosX = CInt(StartPosX + Sx)
                StartPosY = CInt(StartPosY - Sy)

                If SolidMap(StartPosX, StartPosY) = 0 Then
                    If TerrainMap(StartPosX, StartPosY) = 3 Then
                    Else
                        VisibilityMap(StartPosX, StartPosY) = 2               'Can see
                    End If
                ElseIf SolidMap(StartPosX, StartPosY) = 1 Then
                    VisibilityMap(StartPosX, StartPosY) = 2                   'Can see
                    Collision = True
                End If
            End While
        Next
    End Sub
End Module