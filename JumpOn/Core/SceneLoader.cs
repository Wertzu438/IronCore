using JumpOn.CheckPoints;
using JumpOn.Collectables;
using JumpOn.Entities;
using JumpOn.Entities.OtherEntities;
using JumpOn.Interactables;
using JumpOn.Traps;
using JumpOn.UI.InfoBoxes;
using MCR_Engine.Core;
using MCR_Engine.ObjectSystem;
using MCR_Engine.ObjectSystem.BaseObjectTypes;

namespace JumpOn.Core;

public static class SceneLoader
{
    private const string DirectoryName = "Scenes";
    public static Scene Read(string sceneName, Engine engine, MobileInfoBox mobileInfoBox, Clock clock)
    {
        Scene scene;
        using (var reader = new StreamReader($"{DirectoryName}/{sceneName}.txt"))
        {
            if (reader.EndOfStream)
                throw new ArgumentException("invalide format - 0");
            
            string[] line = reader.ReadLine().Split(",");
            if (line.Length == 3 && int.TryParse(line[0], out int width) && int.TryParse(line[1], out int height))
                scene = new Scene(engine.Camera, width, height, Textures.ConvertNullableTexture(engine.Textures.Get(line[2])));
            else throw new ArgumentException("invalide format - A");
            while (!reader.EndOfStream)
            {
                try
                {
                    line = reader.ReadLine().Split(",");
                    int x = int.Parse(line[1]), y = int.Parse(line[2]);
                    GameObject gameObject = (line[0]) switch 
                    {
                        "SolidObj" => new SolidObject(x, y, int.Parse(line[3]), engine.Textures.Get(line[4]), Enum.Parse<Direction>(line[5]), int.Parse(line[6])),
                        "Decoration" => new DecorationObject(x, y, int.Parse(line[3]), engine.Textures.Get(line[4]), Enum.Parse<Direction>(line[5]), int.Parse(line[6])),
                        //INTERACTEBLES
                        "Ladder" => new Ladder(x, y, engine.Textures, int.Parse(line[3])),
                        "Bar" => new Bar(x, y, engine.Textures, int.Parse(line[3])),
                        "Switch" => new Switch(x, y, scene.Objects.Get(scene.Objects.Count - 1), engine.Input, engine.Camera, mobileInfoBox, engine.Textures, clock),
                        //TRAPS
                        "Laser" => new Laser(x, y, int.Parse(line[3]), int.Parse(line[4]), engine.Textures, clock),
                        "ArrowMachine" => new ArrowMachine(x, y, Enum.Parse<Direction>(line[3]), int.Parse(line[4]), engine.Textures, clock),
                        "Spike" => new Spike(x, y, engine.Textures, int.Parse(line[3])),
                        //COLLECTABLES
                        "Goal" => new Goal(x, y, engine.Textures, engine.Input, mobileInfoBox),
                        "Key" => new GameKey(x, y, engine.Textures),
                        "Medkit" => new Medkit(x, y, engine.Textures, engine.Input, mobileInfoBox),
                        "CheckPoint" => new CheckPoint(x, y, engine.Textures),
                        //ENTITIES
                        "Box" => new Box(x, y, clock, engine.Textures),
                        "MovingPlatform" => new MovingPlatform(x, y, double.Parse(line[3]), double.Parse(line[4]), int.Parse(line[5]), clock, engine.Textures, int.Parse(line[6])),
                        _ => throw new ArgumentException("invalide format"),
                    };
                    scene.Objects.Add(gameObject);
                }
                catch (Exception)
                {
                    engine.UpdateRenderAction(ProcessAction.Pause);
                    Console.ResetColor(); Console.Clear();
                    Console.WriteLine($"Failed to load '{line[0]}', Scene: {sceneName}");
                    Thread.Sleep(3000);
                    engine.UpdateRenderAction(ProcessAction.Start);
                }
            }
        }
        return scene;
    }
    public static int GetSceneAmount() => Directory.GetFiles(DirectoryName,  "*.txt").Length - 1;
}