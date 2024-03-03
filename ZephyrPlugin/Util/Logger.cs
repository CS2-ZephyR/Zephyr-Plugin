using System;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace ZephyrPlugin.Util;

public class Logger
{
    private readonly string _moduleName;

    public Logger(string moduleName)
    {
        _moduleName = moduleName;
    }

    public void All(string message)
    {
        Info(message);
        ChatAll(message);
    }

    public void Info(string message)
    {
        SendPrefix();

        SendWithColor(ConsoleColor.Green, "(INFO) ");
        SendWithColor(ConsoleColor.White, message.ReplaceColorTags());

        Console.Write('\n');
    }

    public void Warn(string message)
    {
        SendPrefix();

        SendWithColor(ConsoleColor.Yellow, "(WARN) ");
        SendWithColor(ConsoleColor.White, message.ReplaceColorTags());

        Console.Write('\n');
    }

    public void Error(string message)
    {
        SendPrefix();

        SendWithColor(ConsoleColor.Red, "(ERROR) ");
        SendWithColor(ConsoleColor.White, message.ReplaceColorTags());

        Console.Write('\n');
    }

    public static void PrintLogo()
    {
        SendWithColor(ConsoleColor.Yellow, "___________                        __________            .__           __________ ");
        Console.Write('\n');
        SendWithColor(ConsoleColor.Yellow, @"\__    ___/___ _____    _____      \____    /____ ______ |  |__ ___.__.\______   ");
        Console.Write('\n');
        SendWithColor(ConsoleColor.Yellow, @"  |    |_/ __ \\__  \  /     \       /     // __ \\____ \|  |  <   |  | |       _/");
        Console.Write('\n');
        SendWithColor(ConsoleColor.Yellow, @"  |    |\  ___/ / __ \|  Y Y  \     /     /\  ___/|  |_> >   Y  \___  | |    |   \");
        Console.Write('\n');
        SendWithColor(ConsoleColor.Yellow, @"  |____| \___  >____  /__|_|  /    /_______ \___  >   __/|___|  / ____| |____|_  /");
        Console.Write('\n');
        SendWithColor(ConsoleColor.Yellow, @"             \/     \/      \/             \/   \/|__|        \/\/             \/ ");
        Console.Write('\n');
    }

    private void SendPrefix()
    {
        SendWithColor(ConsoleColor.White, "[");
        SendWithColor(ConsoleColor.Yellow, "Team ZephyR");
        SendWithColor(ConsoleColor.White, " | ");
        SendWithColor(ConsoleColor.Blue, _moduleName);
        SendWithColor(ConsoleColor.White, "] ");
    }

    private static void SendWithColor(ConsoleColor color, string message)
    {
        Console.ForegroundColor = color;
        Console.Write(message);
        Console.ResetColor();
    }

    public static void ChatAll(string message)
    {
        Server.PrintToChatAll($"{{White}}[{{Gold}}Team ZephyR{{White}}] {message}".ReplaceColorTags());
    }

    public static void Chat(CCSPlayerController player, string message)
    {
        player.PrintToChat($"{{White}}[{{Gold}}Team ZephyR{{White}}] {message}".ReplaceColorTags());
    }

    public static void Center(CCSPlayerController player, string message, bool red = false)
    {
        VirtualFunctions.ClientPrint(player.Handle, red ? HudDestination.Alert : HudDestination.Center, message, 0, 0, 0, 0);
    }
    
    public static void CenterAll(string message, bool red = false)
    {
        VirtualFunctions.ClientPrintAll(red ? HudDestination.Alert : HudDestination.Center, message, 0, 0, 0, 0);
    }
}
