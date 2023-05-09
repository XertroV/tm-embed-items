// See https://aka.ms/new-console-template for more information
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.LZO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("error: pass json file");
            // AwaitEnter();
            return;
        }

        GBX.NET.Lzo.SetLzo(typeof(GBX.NET.LZO.MiniLZO));

        var config = JsonConvert.DeserializeObject<JsonConfig>(File.ReadAllText(args[0]));

        var sz = JsonConvert.SerializeObject(config);
        Console.WriteLine(sz + "\n\n");

        var map = GameBox.ParseNode<CGameCtnChallenge>(config.MapPath);

        Console.WriteLine($"embedded count: {map.EmbeddedData.Count}");
        // Console.WriteLine($"embedded count: {}");
        // var itemModels = ;
        uint count = 0;
        foreach (var node in map.GetEmbeddedItemModelHeaders()) {
            Console.WriteLine($"got node type: {node.GetType()}");
            if (node is CGameItemModel im) {
                Console.WriteLine(im.Ident.ToString());
                // var imFull = GameBox.ParseNode<CGameItemModel>(new System.IO.MemoryStream(map.EmbeddedData["Items/" + im.Ident.Id.Replace("\\","/")]));
                // if (imFull is CGameItemModel) {
                //     Console.WriteLine($"Recovered : {imFull.Ident.ToString()}");
                // }
            }
            count++;
        //     // im.Save()
        }
        Console.WriteLine($"Count: {count}");

        var keys2 = map.EmbeddedData.Keys;
        Console.WriteLine("Embed Keys: " + string.Join(", ", keys2));

        // return;

        foreach (var itemSpec in config.Items)
        {
            var cwd = Directory.GetCurrentDirectory();
            Console.WriteLine($"cwd: {cwd}");
            var itemPath = itemSpec.Path;
            CGameItemModel item;
            try {
                item = GameBox.ParseNode<CGameItemModel>(itemPath);
            } catch (Exception e) {
                Console.WriteLine($"Error: couln't read item at {itemPath}. Exception: {e.ToString()}");
                continue;
            }
            var _ident = item.Ident;
            Console.WriteLine("orig ident path: " + _ident.ToString());
            var identPath = itemPath.Replace("\\", "/");

            if (identPath.StartsWith("Items/"))
            {
                identPath = identPath.Substring(6);
            }

            var identPathBs = identPath.Replace("/", "\\");
            var itemIdent = new Ident(identPathBs, _ident.Collection, _ident.Author);
            item.Ident = itemIdent;

            Console.WriteLine($"Wrote item to: {itemPath}");

            var itemFileName = Path.GetFileName(identPath);
            Console.WriteLine("");
            Console.WriteLine($"itemFileName: {itemFileName}");
            Console.WriteLine($"identPath: {identPath}");
            var dirName = "Items/" + identPath.Substring(0, identPath.Length - itemFileName.Length - 1).Replace(@"\", "/");
            Console.WriteLine($"dir: {dirName}");
            Console.WriteLine("");

            //map.EmbeddedData.Add(itemPath, item.ToGbx().ToString().ToB);
            var embedKey = $"{cwd}/{itemPath}".Replace(@"\", "/");
            if (File.Exists(embedKey))
            {
                Console.WriteLine($"Exists: {embedKey}");
            }
            else
            {
                Console.WriteLine($"Does not exist: {embedKey}");
            }
            Console.WriteLine($"embedKey: {embedKey}");
            Console.WriteLine($"itemPath: {itemPath}");
            Console.WriteLine($"dirName: {dirName}");
            map.ImportFileToEmbed(itemPath, dirName, true);

            // var anchoredObj = map.PlaceAnchoredObject(itemIdent, itemSpec.Position.ToVec3(), itemSpec.Rotation.ToVec3(), itemSpec.Pivot.ToVec3());
            // anchoredObj.BlockUnitCoord = itemSpec.Position.ToCoord();
            // Console.WriteLine($"anchoredObj.ItemModel.Id: {anchoredObj.ItemModel.Id}");
            // anchoredObj.

            // Console.WriteLine(item.Ident.ToString());
            // Console.WriteLine(anchoredObj.ToString());
            // Console.WriteLine(anchoredObj.BlockUnitCoord.ToString());
        }

        var keys = map.EmbeddedData.Keys;
        Console.WriteLine("Embed Keys: " + string.Join(", ", keys));

        map.Save(config.MapPath);

        /*
        var clip = map.ClipGroupInGame.Clips[1];

        var triggerStr = clip.Trigger.ToString();

        var coords = clip.Trigger.Coords;
        var cstr = $"Nb: {coords.Length}; ";

        for (int i = 0; i < coords.Length; i++)
        {
            cstr += coords[i].ToString() + ", ";
        }

        Console.WriteLine(cstr);
        map.MapName += "-resave";
        map.Save("c:\\users\\xertrov\\documents\\trackmania\\maps\\my maps\\_itemabuse\\item abuse - 12m4.map.gbx");
        Console.WriteLine($"saved map. size: {map.Size.ToString()}");
        */
        // AwaitEnter();


        // void AwaitEnter()
        // {
        //     Console.WriteLine("Press enter to exit.");
        //     Console.ReadLine();
        // }
    }
}

public class JVec3
{
    public float X, Y, Z;
    public Vec3 ToVec3()
    {
        return new Vec3(X, Y, Z);
    }

    public Byte3 ToCoord()
    {
        return new Byte3(
            (byte)(int)Math.Floor(X / 32.0),
            (byte)(int)Math.Floor(Y / 8.0 + 8.0),
            (byte)(int)Math.Floor(Z / 32.0)
        );
    }
}

public class ItemJson
{
    public string Name;
    public string Path;
    public JVec3 Position;
    public JVec3 Rotation;
    public JVec3 Pivot;
}

public class JsonConfig
{
    public string MapPath;
    public List<ItemJson> Items;
    public List<bool> Blocks;
    // does nothing atm
    public string Env;
    // does nothing atm
    public string MapSuffix;
    // does nothing atm
    public bool CleanBlocks;
    // does nothing atm
    public bool CleanItems;
    // does nothing atm
    public bool ShouldOverwrite;
}
