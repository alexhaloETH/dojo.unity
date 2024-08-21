using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Numerics;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class ManifestProcessor : EditorWindow
{
    private static List<EnumDetails> _allEnums = new();
    private static List<StructDetails> _allStructs = new();
    private static List<ModelDetails> _allModels = new();
    private static List<SystemDetails> _systemFunctions = new();

    private static HashSet<string> knownTypes = new()
        { "u32", "u64", "u128", "bool", "ContractAddress", "felt252", "u16", "u8" };

    private static readonly Dictionary<string, string> TypeMapping = new()
    {
        { "u8", "byte" },
        { "bool", "bool" },
        { "u16", "UInt16" },
        { "u32", "UInt32" },
        { "u64", "UInt64" },
        { "u128", "FieldElement" },
        { "ContractAddress", "FieldElement" },
        { "felt252", "FieldElement" }
    };

    private readonly string[] tabNames = { "Settings", "Systems", "Enums and Structs", "Models" };

    private int currentTab;
    private Vector2 scrollPos;
    private Vector2 scrollPosSubMenu;
    private int selectedFunctionIndex;
    private int selectedModelIndex;
    private int selectedSystemIndex;

    // Overall Settings
    private string filePath = "";
    private string projectName = "";
    public bool useFieldElement = true;

    // Enums and Structs Settings


    // Models Settings
    public string varPrefix = "";


    // System Settings


    [MenuItem("Dojo SDK/Manifest Translation")]
    public static void ShowWindow()
    {
        GetWindow<ManifestProcessor>("Manifest Translation");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space();

        GUILayout.Label("Drag and drop your JSON manifest file here", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        filePath = EditorGUILayout.TextField(filePath);
        if (GUILayout.Button("Browse")) filePath = EditorUtility.OpenFilePanel("Select JSON manifest file", "", "json");
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Process Manifest"))
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                ProcessManifest(filePath);
            else
                EditorUtility.DisplayDialog("Error", "Please select a valid JSON file.", "OK");
        }

        if (_allEnums.Count == 0 && _allStructs.Count == 0 && _allModels.Count == 0 && _systemFunctions.Count == 0)
        {
            EditorGUILayout.LabelField("No data has been processed");
            return;
        }

        EditorGUILayout.Space();
        currentTab = GUILayout.Toolbar(currentTab, tabNames);
        EditorGUILayout.Space();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        switch (currentTab)
        {
            case 0:
                DrawSettingsTab();
                break;
            case 1:
                DrawSystemsTab();
                break;
            case 2:
                DrawEnumsAndStructsTab();
                break;
            case 3:
                DrawModelsTab();
                break;
        }

        EditorGUILayout.EndScrollView();


        if (currentTab != 0) ManifestActions();
    }


    // this needs to be broken down based on the tabs themselves
    private void ManifestActions()
    {
        GUILayout.Space(10);
        EditorGUI.BeginDisabledGroup(_allModels.Count == 0);
        if (GUILayout.Button("Generate Query Manager"))
        {
            // Call your method to generate query manager
        }

        if (GUILayout.Button("Generate Models")) GenerateModels();
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(_allStructs.Count == 0 && _allEnums.Count == 0);
        if (GUILayout.Button("Generate Custom Types Manager"))
        {
            // Call your method to generate custom types manager
        }

        EditorGUI.EndDisabledGroup();

        EditorGUI.BeginDisabledGroup(_systemFunctions.Count == 0);
        if (GUILayout.Button("Generate Dojo Contract Caller")) GenerateSystems();
        EditorGUI.EndDisabledGroup();
    }


    private void DrawSettingsTab()
    {
        GUILayout.Space(20);

        DrawProjectSection();
        GUILayout.Space(30);

        DrawSystemSection();
        GUILayout.Space(30);

        DrawEnumsAndStructsSection();
        GUILayout.Space(30);

        DrawModelsSection();
        GUILayout.Space(30);


        DrawLoadPreviousSection();


        void DrawProjectSection()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Project", EditorStyles.boldLabel);
            GUILayout.Space(10);
            projectName = EditorGUILayout.TextField("Project Name", projectName);
            useFieldElement = EditorGUILayout.Toggle("Use FieldElements", useFieldElement);
            EditorGUILayout.EndVertical();
        }

        void DrawSystemSection()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("System", EditorStyles.boldLabel);
            GUILayout.Space(10);
            varPrefix = EditorGUILayout.TextField("Variable Prefix", varPrefix);
            EditorGUILayout.EndVertical();
        }

        void DrawEnumsAndStructsSection()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Enums and Structs", EditorStyles.boldLabel);
            GUILayout.Space(10);
            varPrefix = EditorGUILayout.TextField("Test", varPrefix);
            EditorGUILayout.EndVertical();
        }

        void DrawModelsSection()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Models", EditorStyles.boldLabel);
            GUILayout.Space(10);
            varPrefix = EditorGUILayout.TextField("Variable Prefix", varPrefix);
            EditorGUILayout.EndVertical();
        }

        void DrawLoadPreviousSection()
        {
            EditorGUILayout.BeginVertical("box");
            GUILayout.Label("Load Previous (Stretch Goals)", EditorStyles.boldLabel);
            GUILayout.Space(10);
            varPrefix = EditorGUILayout.TextField("Test", varPrefix);
            if (GUILayout.Button("Test")) TestFunctionForTestingTests();
            EditorGUILayout.EndVertical();
        }

        #region Full Testing

        void TestFunctionForTestingTests()
        {
         
        }

        #endregion
    }


    /// <summary>
    /// Systems Tabs is where are all the functions processed from the manifest get shown
    /// </summary>
    private void DrawSystemsTab()
    {
        if (_systemFunctions.Count == 0)
        {
            EditorGUILayout.LabelField("No System functions found in the manifest.");
            return;
        }

        EditorGUILayout.BeginHorizontal();

        // Vertical toolbar for systems list
        EditorGUILayout.BeginVertical(GUILayout.Width(150));
        for (var i = 0; i < _systemFunctions.Count; i++)
            if (GUILayout.Toggle(selectedSystemIndex == i, _systemFunctions[i].Name, EditorStyles.toolbarButton))
                selectedSystemIndex = i;
        // Reset function selection when changing system
        EditorGUILayout.EndVertical();

        // Vertical toolbar for functions within the selected system
        EditorGUILayout.BeginVertical(GUILayout.Width(150));
        if (_systemFunctions.Count > 0)
        {
            var selectedSystem = _systemFunctions[selectedSystemIndex];
            for (var i = 0; i < selectedSystem.Functions.Count; i++)
                if (GUILayout.Toggle(selectedFunctionIndex == i, selectedSystem.Functions[i].Name,
                        EditorStyles.toolbarButton))
                    selectedFunctionIndex = i;
        }

        EditorGUILayout.EndVertical();

        // Scroll view for function details
        scrollPosSubMenu = EditorGUILayout.BeginScrollView(scrollPosSubMenu);

        if (_systemFunctions.Count > 0)
        {
            var selectedSystem = _systemFunctions[selectedSystemIndex];

            if (selectedSystem.Functions.Count > 0)
            {
                if (selectedFunctionIndex < 0 || selectedFunctionIndex >= selectedSystem.Functions.Count)
                    selectedFunctionIndex = 0;

                var selectedFunction = selectedSystem.Functions[selectedFunctionIndex];

                EditorGUILayout.LabelField("Function: " + selectedFunction.Name, EditorStyles.boldLabel);

                GUILayout.Space(10);

                foreach (var argument in selectedFunction.Arguments)
                    EditorGUILayout.LabelField($"{argument.Item1}: {argument.Item2}");


                if (selectedFunction.Outputs.Count > 0)
                {
                    GUILayout.Space(10);

                    foreach (var output in selectedFunction.Outputs) EditorGUILayout.LabelField($"Returns: {output}");
                }

                GUILayout.Space(10);

                selectedFunction.Generate = EditorGUILayout.Toggle(
                    new GUIContent("Generate", "Check this to generate the function."), selectedFunction.Generate);
                selectedFunction.invokeFunction = EditorGUILayout.Toggle(
                    new GUIContent("Invoke Function", "Check this to invoke the function after generation."),
                    selectedFunction.invokeFunction);
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEnumsAndStructsTab()
    {
        if (_allEnums.Count == 0 && _allStructs.Count == 0)
        {
            EditorGUILayout.LabelField("No Enums or Structs found in the manifest.");
            return;
        }

        EditorGUILayout.BeginHorizontal();

        // Vertical toolbar for enums and structs
        EditorGUILayout.BeginVertical(GUILayout.Width(150));
        EditorGUILayout.LabelField("Enums");
        for (var i = 0; i < _allEnums.Count; i++)
            if (GUILayout.Toggle(selectedModelIndex == i, _allEnums[i].Name, EditorStyles.toolbarButton))
                selectedModelIndex = i;

        GUILayout.Space(30);

        EditorGUILayout.LabelField("Structs");
        for (var i = 0; i < _allStructs.Count; i++)
            if (GUILayout.Toggle(selectedModelIndex == i + _allEnums.Count, _allStructs[i].Name,
                    EditorStyles.toolbarButton))
                selectedModelIndex = i + _allEnums.Count;
        EditorGUILayout.EndVertical();

        // Scroll view for enum/struct details
        scrollPosSubMenu = EditorGUILayout.BeginScrollView(scrollPosSubMenu);

        if (selectedModelIndex < _allEnums.Count)
        {
            var enumDetails = _allEnums[selectedModelIndex];
            EditorGUILayout.LabelField("Enum: " + enumDetails.Name, EditorStyles.boldLabel);
            foreach (var value in enumDetails.Values) EditorGUILayout.LabelField($"Value: {value}");

            enumDetails.Generate = EditorGUILayout.Toggle("Generate", enumDetails.Generate);
        }
        else if (selectedModelIndex - _allEnums.Count < _allStructs.Count)
        {
            var structDetails = _allStructs[selectedModelIndex - _allEnums.Count];
            EditorGUILayout.LabelField("Struct: " + structDetails.Name, EditorStyles.boldLabel);

            foreach (var variable in structDetails.Members)
                EditorGUILayout.LabelField($"{variable.Item1}: {variable.Item2}");

            structDetails.Generate = EditorGUILayout.Toggle("Generate", structDetails.Generate);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawModelsTab()
    {
        if (_allModels.Count == 0)
        {
            EditorGUILayout.LabelField("No Models found in the manifest.");
            return;
        }

        GUILayout.Space(10);

        varPrefix = EditorGUILayout.TextField("Variable Prefix", varPrefix);

        GUILayout.Space(20);

        EditorGUILayout.BeginHorizontal();

        // Vertical toolbar for models list
        EditorGUILayout.BeginVertical(GUILayout.Width(150));
        for (var i = 0; i < _allModels.Count; i++)
            if (GUILayout.Toggle(selectedModelIndex == i, _allModels[i].Name, EditorStyles.toolbarButton))
                selectedModelIndex = i;
        EditorGUILayout.EndVertical();

        // Scroll view for model details
        scrollPosSubMenu = EditorGUILayout.BeginScrollView(scrollPosSubMenu);

        if (_allModels.Count > 0)
        {
            var model = _allModels[selectedModelIndex];

            EditorGUILayout.LabelField("Model: " + model.Name, EditorStyles.boldLabel);
            GUILayout.Space(10);
            foreach (var variable in model.Members) EditorGUILayout.LabelField($"{variable.Item1}: {variable.Item2}");
            GUILayout.Space(10);
            model.Generate = EditorGUILayout.Toggle("Generate", model.Generate);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
    }


    #region Generation Region

    /// <summary>
    ///     Processes the JSON manifest file to extract models, system functions, structs, and enums.
    /// </summary>
    /// <param name="path">Path to the JSON manifest file.</param>
    private void ProcessManifest(string path)
    {
        Debug.Log("Processing manifest...");
        var jsonString = File.ReadAllText(path);
        var manifestData = JObject.Parse(jsonString);

        // Reset static collections
        _allEnums.Clear();
        _allStructs.Clear();
        _allModels.Clear();
        _systemFunctions.Clear();
        knownTypes = new HashSet<string> { "u32", "u64", "u128", "bool", "ContractAddress", "felt252", "u8", "u16" };

        // Extract models
        _allModels = ParseModels(manifestData);

        // Extract system functions
        _systemFunctions = ExtractSystemFunctions(manifestData, new List<string> { "upgrade", "verify", "world" });

        // Extract structs and enums
        (_allStructs, _allEnums) = ExtractStructsAndEnums(manifestData);
        Debug.Log("Processing completed.");
    }

    /// <summary>
    ///     Parses the models from the manifest data.
    /// </summary>
    /// <param name="manifestData">JSON object representing the manifest data.</param>
    /// <returns>List of parsed models.</returns>
    private List<ModelDetails> ParseModels(JObject manifestData)
    {
        var models = new List<ModelDetails>();

        if (manifestData["models"] == null)
        {
            Debug.LogError("No 'models' field found in the JSON.");
            return models;
        }

        foreach (var model in manifestData["models"])
        {
            var newModel = new ModelDetails
            {
                Name = model["name"]?.ToString().Split(new[] { "::" }, StringSplitOptions.None)[^1]
            };

            if (model["members"] != null)
                foreach (var member in model["members"])
                {
                    var varName = member["name"]?.ToString();
                    var varType = member["type"]?.ToString().Split(new[] { "::" }, StringSplitOptions.None)[^1];
                    newModel.Members.Add(new Tuple<string, string>(varName, varType));
                    knownTypes.Add(varType);
                }

            models.Add(newModel);
        }

        Debug.Log("Models parsed: " + models.Count);
        return models;
    }

    /// <summary>
    ///     Extracts system functions from the manifest data.
    /// </summary>
    /// <param name="manifestData">JSON object representing the manifest data.</param>
    /// <param name="ignoreList">List of system names to ignore.</param>
    /// <returns>List of parsed system functions.</returns>
    private List<SystemDetails> ExtractSystemFunctions(JObject manifestData, List<string> ignoreList)
    {
        var systems = new List<SystemDetails>();

        if (manifestData["contracts"] == null)
        {
            Debug.LogError("No 'contracts' field found in the JSON.");
            return systems;
        }

        foreach (var contract in manifestData["contracts"])
        {
            var newSystem = new SystemDetails
            {
                Name = contract["name"]?.ToString().Split(new[] { "::" }, StringSplitOptions.None)[^1]
            };

            var abi = (JArray)contract["abi"];
            foreach (var item in abi)
                if (item["type"]?.ToString() == "interface")
                    foreach (var subItem in item["items"])
                        if (subItem["type"]?.ToString() == "function" &&
                            !ignoreList.Contains(subItem["name"]?.ToString()))
                        {
                            var functionDetails = new FunctionDetails
                            {
                                Name = subItem["name"]?.ToString()
                            };

                            foreach (var arg in subItem["inputs"])
                            {
                                var argName = arg["name"]?.ToString();
                                var argType =
                                    arg["type"]?.ToString().Split(new[] { "::" }, StringSplitOptions.None)[^1];
                                functionDetails.Arguments.Add(new Tuple<string, string>(argName, argType));
                                knownTypes.Add(argType);
                            }

                            foreach (var output in subItem["outputs"] ?? new JArray())
                            {
                                var outputType = output["type"]?.ToString()
                                    .Split(new[] { "::" }, StringSplitOptions.None)[^1];
                                functionDetails.Outputs.Add(outputType);
                                knownTypes.Add(outputType);
                            }

                            if (functionDetails.Outputs.Count > 0) functionDetails.invokeFunction = false;

                            newSystem.Functions.Add(functionDetails);
                        }

            if (newSystem.Functions.Count > 0) systems.Add(newSystem);
        }

        Debug.Log("System functions parsed: " + systems.Count);
        return systems;
    }

    /// <summary>
    ///     Extracts structs and enums from the manifest data.
    /// </summary>
    /// <param name="manifestData">JSON object representing the manifest data.</param>
    /// <returns>Tuple containing lists of parsed structs and enums.</returns>
    private Tuple<List<StructDetails>, List<EnumDetails>> ExtractStructsAndEnums(JObject manifestData)
    {
        var structs = new List<StructDetails>();
        var enums = new List<EnumDetails>();

        if (manifestData["contracts"] == null)
        {
            Debug.LogError("No 'contracts' field found in the JSON.");
            return new Tuple<List<StructDetails>, List<EnumDetails>>(structs, enums);
        }

        var existingStructNames = new HashSet<string>();
        var existingEnumNames = new HashSet<string>();

        foreach (var contract in manifestData["contracts"])
        {
            var abi = (JArray)contract["abi"];
            foreach (var item in abi)
                if (item["type"]?.ToString() == "struct")
                {
                    var structName = item["name"]?.ToString().Split(new[] { "::" }, StringSplitOptions.None)[^1];
                    if (!existingStructNames.Contains(structName))
                    {
                        var structDetails = new StructDetails
                        {
                            Name = structName
                        };

                        if (item["members"] != null)
                            foreach (var member in item["members"])
                            {
                                var varName = member["name"]?.ToString();
                                var varType =
                                    member["type"]?.ToString().Split(new[] { "::" }, StringSplitOptions.None)[^1];
                                structDetails.Members.Add(new Tuple<string, string>(varName, varType));
                                knownTypes.Add(varType);
                            }
                        else
                            Debug.LogWarning($"Struct '{structDetails.Name}' has no members.");

                        structs.Add(structDetails);
                        existingStructNames.Add(structName);
                        knownTypes.Add(structDetails.Name);
                    }
                }
                else if (item["type"]?.ToString() == "enum")
                {
                    var enumName = item["name"]?.ToString().Split(new[] { "::" }, StringSplitOptions.None)[^1];
                    if (!existingEnumNames.Contains(enumName))
                    {
                        var enumDetails = new EnumDetails
                        {
                            Name = enumName
                        };

                        if (item["variants"] != null)
                            foreach (var variant in item["variants"])
                                enumDetails.Values.Add(variant["name"]?.ToString());
                        else
                            Debug.LogWarning($"Enum '{enumDetails.Name}' has no variants.");

                        enums.Add(enumDetails);
                        existingEnumNames.Add(enumName);
                        knownTypes.Add(enumDetails.Name);
                    }
                }
        }

        return new Tuple<List<StructDetails>, List<EnumDetails>>(structs, enums);
    }

    /// <summary>
    /// This Will return the type variant from Cairo to C#
    /// </summary>
    /// <param name="type">Cairo type in string format</param>
    /// <param name="useFieldElement">Bool, Dojo SDK Mandates that u128 should be treated as Bigintegers, sometimes FieldElements are simpler, if true it uses FieldElements if not uses Bigintegers</param>
    /// <returns></returns>
    public static string GetFieldType(string type, bool useFieldElement)
    {
        if (!useFieldElement && type == "u128") return "BigInteger";

        if (TypeMapping.TryGetValue(type, out var mappedType)) return mappedType;
        return type;
    }


    private void GenerateSystems()
    {
        Debug.Log("Generating systems...");
        var systemsPath = Path.Combine("Assets", projectName, "Scripts", "Systems");
        if (!Directory.Exists(systemsPath)) Directory.CreateDirectory(systemsPath);

        foreach (var system in _systemFunctions) system.GenerateSystemScript(systemsPath);
    }

    private void GenerateModels()
    {
        Debug.Log("Generating models...");
        var modelsPath = Path.Combine("Assets", projectName, "Scripts", "Models");
        if (!Directory.Exists(modelsPath)) Directory.CreateDirectory(modelsPath);

        foreach (var model in _allModels)
            if (model.Generate)
                model.GenerateScript(modelsPath, this);
    }
    #endregion
}

public abstract class BaseDetails
{
    public string Name { get; set; }
    public List<Tuple<string, string>> Members { get; set; } = new();
    public bool Generate { get; set; } = true;

    /// <summary>
    ///     Converts a snake_case string to CamelCase or camelCase.
    /// </summary>
    /// <param name="input">The input string in snake_case format.</param>
    /// <param name="capitalizeFirst">Whether to capitalize the first letter (CamelCase) or not (camelCase).</param>
    /// <returns>The converted string in CamelCase or camelCase.</returns>
    public string ConvertToCamelCase(string input, bool capitalizeFirst = true)
    {
        var words = input.Split('_');
        for (var i = 0; i < words.Length; i++) words[i] = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(words[i]);

        var result = string.Join(string.Empty, words);
        if (!capitalizeFirst && result.Length > 0) result = char.ToLower(result[0]) + result[1..];

        return result;
    }

    /// <summary>
    ///     Converts a snake_case string to PascalCase.
    /// </summary>
    /// <param name="input">The input string in snake_case format.</param>
    /// <returns>The converted string in PascalCase.</returns>
    public string ToPascalCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var words = input.Split('_');
        for (var i = 0; i < words.Length; i++)
            if (words[i].Length > 0)
                words[i] = char.ToUpper(words[i][0]) + words[i][1..].ToLower();
        return string.Join(string.Empty, words);
    }
}

public class FunctionDetails : BaseDetails
{
    public bool invokeFunction = true;

    public List<Tuple<string, string>> Arguments { get; set; } = new();
    public List<string> Outputs { get; set; } = new();
}


public class ModelDetails : BaseDetails
{
    public void GenerateScript(string modelsPath, ManifestProcessor manifestProcessor)
    {
        var modelName = ConvertToCamelCase(Name);
        var filePath = Path.Combine(modelsPath, modelName + ".cs");

        if (File.Exists(filePath))
        {
            UpdateModelScript(filePath, manifestProcessor);
        }
        else
        {
            var scriptContent = GenerateModelScript(manifestProcessor);
            File.WriteAllText(filePath, scriptContent);
            Debug.Log($"Generated model script: {filePath}");
        }
    }

    private string GenerateModelScript(ManifestProcessor manifestProcessor)
    {
        var template = @"using Dojo;
            using Dojo.Starknet;
            using Dojo.Torii;
            using System.Numerics;

            public class {MODEL_NAME} : ModelInstance
            {
               #region GeneratedRegion                      
    
            {MODEL_FIELDS}

                #endregion
                                                      
                private void Update()
                {
        
                }

                void Start()
                {

                }

                public override void OnUpdate(Model model)
                {
                    base.OnUpdate(model);
                }
            }";

        var fields = "";
        foreach (var member in Members)
        {
            var fieldType = ManifestProcessor.GetFieldType(member.Item2, manifestProcessor.useFieldElement);
            fields += $"\t[ModelField(\"{member.Item1}\")]\n";
            fields += $"\tpublic {fieldType} {manifestProcessor.varPrefix}{ToPascalCase(member.Item1)};\n\n";
        }

        return template.Replace("{MODEL_NAME}", Name).Replace("{MODEL_FIELDS}", fields);
    }

    private void UpdateModelScript(string filePath, ManifestProcessor manifestProcessor)
    {
        var lines = File.ReadAllLines(filePath);
        var inGeneratedRegion = false;
        var newLines = new List<string>();

        foreach (var line in lines)
        {
            if (line.Contains("#region GeneratedRegion"))
            {
                inGeneratedRegion = true;
                newLines.Add(line);
                newLines.Add("");
                foreach (var member in Members)
                {
                    var fieldType = ManifestProcessor.GetFieldType(member.Item2, manifestProcessor);
                    newLines.Add($"\t[ModelField(\"{member.Item1}\")]");
                    newLines.Add($"\tpublic {fieldType} {ToPascalCase(member.Item1)};");
                    newLines.Add("");
                }
            }
            else if (line.Contains("#endregion"))
            {
                inGeneratedRegion = false;
            }

            if (!inGeneratedRegion) newLines.Add(line);
        }

        File.WriteAllLines(filePath, newLines);
    }
}

public class SystemDetails : BaseDetails
{
    public List<FunctionDetails> Functions { get; set; } = new List<FunctionDetails>();

    public void GenerateSystemScript(string systemsPath)
    {
        string systemName = ConvertToCamelCase(Name);
        string filePath = Path.Combine(systemsPath, systemName + "ActionsContract.cs");

        string scriptContent = GenerateSystemScriptContent();
        File.WriteAllText(filePath, scriptContent);
        Debug.Log($"Generated system script: {filePath}");
    }

    private string GenerateSystemScriptContent()
    {
        string template = @"
            using Dojo;
            using Dojo.Starknet;
            using Dojo.Torii;
            using System;
            using System.Collections.Generic;
            using System.Numerics;
            using System.Threading.Tasks;

            public static class {SYSTEM_NAME}ActionsContract
            {
                public enum FunctionNames
                {
                    {FUNCTION_ENUMS}
                }

                public static string EnumToString(FunctionNames functionName)
                {
                    switch (functionName)
                    {
                        {ENUM_TO_STRING_CASES}
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                {STRUCTS}
            }";

        string functionEnums = "";
        string enumToStringCases = "";
        string structs = "";

        foreach (var function in Functions)
        {
            string functionName = ConvertToCamelCase(function.Name);
            functionEnums += $"{functionName},\n";
            enumToStringCases += $"case FunctionNames.{functionName}:\n    return \"{function.Name.ToLower()}\";\n";

            string structFields = "public EndpointDojoCallStruct endpointData;\n";
            foreach (var arg in function.Arguments)
            {
                string fieldType = ManifestProcessor.GetFieldType(arg.Item2, true);
                string fieldName = ConvertToCamelCase(arg.Item1, false);
                structFields += $"public {fieldType} {fieldName};\n";
            }

            string constructorArgs = "EndpointDojoCallStruct endpointData";
            string constructorBody = "this.endpointData = endpointData;\n";
            foreach (var arg in function.Arguments)
            {
                string fieldName = ConvertToCamelCase(arg.Item1, false);
                constructorArgs += $", {ManifestProcessor.GetFieldType(arg.Item2, true)} {fieldName}";
                constructorBody += $"this.{fieldName} = {fieldName};\n";
            }

            string structTemplate = function.invokeFunction
                            ? $@"
                public struct {functionName}InvokeStruct
                {{
                    public bool invokeCall;
                    {structFields}

                    public {functionName}InvokeStruct({constructorArgs})
                    {{
                        this.invokeCall = true;
                        {constructorBody}
                    }}
                }}"
                            : $@"
                public struct {functionName}ViewStruct
                {{
                    {structFields}

                    public {functionName}ViewStruct({constructorArgs})
                    {{
                        {constructorBody}
                    }}

                    public async Task<FieldElement[]> {functionName}Call(JsonRpcClient jsonRpcClient, dojo.BlockId blockData)
                    {{
                        var callData = new List<dojo.FieldElement>();

                        DojoContractCommunication.ProcessStructValues(this, callData);

                        var call = new dojo.Call
                        {{
                            calldata = callData.ToArray(),
                            selector = endpointData.functionName,
                            to = new FieldElement(endpointData.addressOfSystem).Inner
                        }};

                        return await jsonRpcClient.Call(call, blockData);
                    }}
                }}";

            structs += structTemplate;
        }

        return template.Replace("{SYSTEM_NAME}", Name)
            .Replace("{FUNCTION_ENUMS}", functionEnums)
            .Replace("{ENUM_TO_STRING_CASES}", enumToStringCases)
            .Replace("{STRUCTS}", structs);
    }
}


public class StructDetails : BaseDetails
{
}

public class EnumDetails : BaseDetails
{
    public List<string> Values { get; set; } = new();
}