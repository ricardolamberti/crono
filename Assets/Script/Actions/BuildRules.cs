using System.Collections.Generic;



public static class BuildRules
{
    public static readonly Dictionary<string, BuildRequirement> buildCosts = new()
    {
        { "worker", new BuildRequirement {
            food = 1,
            housing = 1,
            requiresTownhall = true
        }},
        { "barracks", new BuildRequirement {
            wood = 30,
            gold = 10
        }},
        { "warrior", new BuildRequirement {
            food = 1,
            housing = 1,
            requiresBarracks = true,
            barracksUnits = 1
        }},
        { "scientist", new BuildRequirement {
            food = 1,
            crono = 5,
            housing = 1,
            academicUnits = 1,
            requiresAcademy = true
        }},
        { "farm", new BuildRequirement {
            wood = 20
        }},
        { "mine", new BuildRequirement {
            wood = 20
        }},
        { "advanced_mine", new BuildRequirement {
            wood = 40,
            gold = 20
        }},
        { "lumbermill", new BuildRequirement {
            wood = 15,
            gold = 5
        }},
        { "acadamy", new BuildRequirement {
            wood = 30,
            gold = 20
        }},
        { "townhall", new BuildRequirement {
            wood = 0,
            gold = 0
        }},
        { "crono_extractor", new BuildRequirement {
            gold = 20,
            wood = 20,
            sciencePoints = 10
        }},
        { "house", new BuildRequirement {
            wood = 20,
            gold = 5
        }}
    };


    public static BuildRequirement GetRequirements(string code)
    {
        if (buildCosts.TryGetValue(code, out var req))
            return req;

        return new BuildRequirement(); // Requerimientos nulos si no se encuentra
    }

    public static BuildRequirement GetRequirements(Character.Type type)
    {
        return type switch
        {
            Character.Type.Worker => GetRequirements("worker"),
            Character.Type.Warrior => GetRequirements("warrior"),
            Character.Type.Scientist => GetRequirements("scientist"),
            _ => new BuildRequirement()
        };
    }

    public static BuildRequirement GetRequirements(CharacterRole role)
    {
        return GetRequirements(role.Code);
    }


    public static BuildRequirement TakeRequirements(string code)
    {
        if (buildCosts.TryGetValue(code, out var req))
        {
            GameState.playerResources.Consume(req);
            return req;

        }

        return new BuildRequirement(); // Requerimientos nulos si no se encuentra
    }

    public static BuildRequirement TakeRequirements(Character.Type type)
    {
        return type switch
        {
            Character.Type.Worker => TakeRequirements("worker"),
            Character.Type.Warrior => TakeRequirements("warrior"),
            Character.Type.Scientist => TakeRequirements("scientist"),
            _ => new BuildRequirement()
        };
    }

    public static BuildRequirement TakeRequirements(CharacterRole role)
    {
        return TakeRequirements(role.Code);
    }
}