using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UndertaleModLib.Util;
using System.Text.RegularExpressions;
using System.Linq;

// Prefire checks
EnsureDataLoaded();
const string expectedDisplayName = "DELTARUNE Chapter ([1-4])";
var displayName = Data?.GeneralInfo?.DisplayName?.Content;
if (!Regex.IsMatch(displayName, expectedDisplayName, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500)))
{
    ScriptError($"Error 0: data file display name does not match expected: '{expectedDisplayName}', actual display name: '{displayName}'.");
    return;
}
ushort ch_no = ushort.Parse(Regex.Match(displayName, expectedDisplayName, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500)).Groups[1].Captures[0].Value);

// Begin edit
ScriptMessage($"Adding mod menu to '{displayName}'...");

// Load texture file
Dictionary<string, UndertaleEmbeddedTexture> textures = new Dictionary<string, UndertaleEmbeddedTexture>();

UndertaleEmbeddedTexture modmenuTexturePage = new UndertaleEmbeddedTexture();
modmenuTexturePage.TextureData.Image = GMImage.FromPng(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(ScriptPath), "modmenu.png")));
Data.EmbeddedTextures.Add(modmenuTexturePage);
textures.Add(Path.GetFileName(Path.Combine(Path.GetDirectoryName(ScriptPath), "modmenu.png")), modmenuTexturePage);

UndertaleTexturePageItem AddNewTexturePageItem(ushort sourceX, ushort sourceY, ushort sourceWidth, ushort sourceHeight)
{
    ushort targetX = 0;
    ushort targetY = 0;
    ushort targetWidth = sourceWidth;
    ushort targetHeight = sourceHeight;
    ushort boundingWidth = sourceWidth;
    ushort boundingHeight = sourceHeight;
    var texturePage = textures["modmenu.png"];

    UndertaleTexturePageItem tpItem = new() 
    { 
        SourceX = sourceX, 
        SourceY = sourceY, 
        SourceWidth = sourceWidth, 
        SourceHeight = sourceHeight, 
        TargetX = targetX, 
        TargetY = targetY, 
        TargetWidth = targetWidth, 
        TargetHeight = targetHeight, 
        BoundingWidth = boundingWidth, 
        BoundingHeight = boundingHeight, 
        TexturePage = texturePage,
        Name = new UndertaleString($"PageItem {Data.TexturePageItems.Count}")
    };
    Data.TexturePageItems.Add(tpItem);
    return tpItem;
}

UndertaleTexturePageItem pg_modsbt1 = AddNewTexturePageItem(0, 0, 33, 24);
UndertaleTexturePageItem pg_modsbt2 = AddNewTexturePageItem(0, 24, 33, 24);
UndertaleTexturePageItem pg_modsbt3 = AddNewTexturePageItem(0, 48, 33, 24);
UndertaleTexturePageItem pg_modsdesc = AddNewTexturePageItem(33, 0, 35, 18);

// add 'mods' button
{
    UndertaleSprite referenceSprite = Data.Sprites.ByName("spr_darkconfigbt");
    var name = Data.Strings.MakeString("spr_darkmodsbt");
    uint width = referenceSprite.Width;
    uint height = referenceSprite.Height;
    ushort marginLeft = 0;
    int marginRight = (int)width - 1;
    ushort marginTop = 0;
    int marginBottom = (int)height - 1;

    var sItem = new UndertaleSprite { Name = name, Width = width, Height = height, MarginLeft = marginLeft, MarginRight = marginRight, MarginTop = marginTop, MarginBottom = marginBottom };

    UndertaleTexturePageItem[] spriteTextures = { pg_modsbt1, pg_modsbt2, pg_modsbt3 };
    foreach (var spriteTexture in spriteTextures) 
    {
        sItem.Textures.Add(new UndertaleSprite.TextureEntry() { Texture = spriteTexture });
    }
    Data.Sprites.Add(sItem);
}

// add 'mods' menu description
{
    UndertaleSprite spr_darkmenudesc = Data.Sprites.ByName("spr_darkmenudesc");
    spr_darkmenudesc.Textures.Add(new UndertaleSprite.TextureEntry() { Texture = pg_modsdesc });
}

// Code edits
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data){
    ThrowOnNoOpFindReplace = true
};

// Add menu create code
importGroup.QueueAppend("gml_Object_obj_darkcontroller_Create_0", @"
    
    global.modmenuno = 0;
    global.modsubmenuno = -1;
    global.modsubmenuselected = false;
    global.modsubmenuscroll = 0;
    global.modmenu_data = array_create(0);
");

Func<string,string,string> ds_map_find_value_lang =
    (id, key) => @$"(ds_map_exists({id}, {key} + ""_"" + global.lang) ? ds_map_find_value({id}, {key} + ""_"" + global.lang) :  ds_map_find_value({id}, {key} + ""_en""))";

// Add menu draw code
importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_darkcontroller_Draw_0", "msprite[4] = spr_darkconfigbt;", @"
    msprite[4] = spr_darkconfigbt;
    msprite[5] = spr_darkmodsbt;
    ");
importGroup.QueueFindReplace("gml_Object_obj_darkcontroller_Draw_0", "i = 0; i < 5; i += 1)", "i = 0; i < (array_length(global.modmenu_data) > 0 ? 6 : 5); i += 1)");
importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_darkcontroller_Draw_0", "spritemx = -100;", "spritemx = (array_length(global.modmenu_data) > 0 ? -80 : -100);");
importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_darkcontroller_Draw_0",
    "draw_sprite_ext(msprite[i], off, xx + 120 + (i * 100) + spritemx, (yy + tp) - 60, 2, 2, 0, c_white, 1);",
    "draw_sprite_ext(msprite[i], off, xx + (array_length(global.modmenu_data) > 0 ? (110 + (i * 80)) : (120 + (i * 100))) + spritemx, (yy + tp) - 60, 2, 2, 0, c_white, 1);");
string back_text = ch_no >= 2 ? "back_text" : "scr_84_get_lang_string(\"obj_darkcontroller_slash_Draw_0_gml_96_0\")";
importGroup.QueueAppend("gml_Object_obj_darkcontroller_Draw_0", @$"
    if (global.menuno == 6)
    {{
        draw_set_color(c_black);
        
        if (global.lang == ""ja"")
        {{
            draw_rectangle(xx + 60, yy + 85, xx + 580, yy + 412, false);
            scr_darkbox(xx + 50, yy + 75, xx + 590, yy + 422);
        }}
        else
        {{
            draw_rectangle(xx + 60, yy + 90, xx + 580, yy + 410, false);
            scr_darkbox(xx + 50, yy + 80, xx + 590, yy + 420);
        }}

        // top row buttons
        var isSubmenu = (global.modsubmenuno >= 0);
        var isMenuLonely = array_length(global.modmenu_data) == 1;

        var startPadding = 0;
        if (!isMenuLonely)
        {{
            var xAcc = 0;
            for (var i = 0; i < array_length(global.modmenu_data); i++)
            {{
                var menu_data = global.modmenu_data[i];
                xAcc += {ds_map_find_value_lang("menu_data", @"""title_size""")} + 25;
            }}
            if (xAcc <= 410)
                startPadding = (410 - xAcc - 45) / 2;
        }}
        else
            startPadding = (410 - {ds_map_find_value_lang("global.modmenu_data[0]", @"""title_size""")}) / 2;
        
        draw_set_color(c_white);
        var xAcc = 0;
        var xSelAcc = 0;
        var isHitMenuNo = false;
        
        for (var i = 0; i < array_length(global.modmenu_data); i++)
        {{
            var menu_data = global.modmenu_data[i];
            var title_size = {ds_map_find_value_lang("menu_data", @"""title_size""")};
            if (isSubmenu)
            {{
                if (isMenuLonely)
                    draw_set_color(c_white);
                else if (global.modmenuno == i)
                    draw_set_color(c_orange);
                else
                    draw_set_color(c_gray);
            }}
        
            draw_text(xx + 110 + startPadding + xAcc, yy + 100 + !isMenuLonely * 10, string_hash_to_newline(string_upper({ds_map_find_value_lang("menu_data", @"""title""")})));
            xAcc += title_size + 45;
            if (!isHitMenuNo && !isSubmenu) {{
                if (global.modmenuno == i)
                    isHitMenuNo = true;
                else
                    xSelAcc += title_size + 45;
            }}
        }}

        if (!isSubmenu)
            draw_sprite(spr_heart, 0, xx + 85 + startPadding + xSelAcc, yy + 120);

        // form buttons
        var _xPos = (global.lang == ""en"") ? (xx + 170) : (xx + 150);
        var _heartXPos = (global.lang == ""en"") ? (xx + 145) : (xx + 125);
        var _selectXPos = (global.lang == ""ja"" && global.is_console) ? (xx + 385) : (xx + 430);

        draw_set_color(c_white);

        if (!isSubmenu)
            draw_set_color(c_gray);

        var form_data = ds_map_find_value(global.modmenu_data[global.modmenuno], ""form"");

        if (array_length(form_data) >= 0)
        {{
            for (var i = global.modsubmenuscroll; i < min(global.modsubmenuscroll + 7, array_length(form_data) + 1 /* (back button) */); i++)
            {{
                if (i >= array_length(form_data))
                {{
                    draw_set_color(c_white);
                    draw_text(_xPos, yy + 150 + (i - global.modsubmenuscroll) * 35, string_hash_to_newline({back_text})); // Back
                    continue;
                }}

                if (global.modsubmenuselected && global.modsubmenuno == i)
                    draw_set_color(c_yellow);
                else
                    draw_set_color(c_white);

                var row_data = form_data[i];
                draw_text(_xPos, yy + 150 + (i - global.modsubmenuscroll) * 35, string_hash_to_newline({ds_map_find_value_lang("row_data", @"""title""")}));

                var value = variable_instance_get(global, ds_map_find_value(row_data, ""value_name""));
                var ranges = string_split({ds_map_find_value_lang("row_data", @"""value_range""")}, "";"");
                var valueString = """";

                for (var j = 0; j < array_length(ranges); j++) {{
                    var range = ranges[j];
                    if (string_ends_with(range, ""%"")) {{
                        var minMax = string_split(string_replace(range, ""%"", """"), ""-"");
                        if (value * 100 <= minMax[1] || j+1 == array_length(ranges)) {{
                            valueString = string_trim(string_format(value * 100, 3, value < 0.2 ? 1 : 0) + ""%"");
                            break;
                        }}
                    }} else if (string_pos(""="", range)) {{
                        var labelValue = string_split(range, ""="");
                        if (value == real(labelValue[1]) || j+1 == array_length(ranges)) {{
                            valueString = labelValue[0];
                            break;
                        }}
                    }}
                }}

                draw_text(_selectXPos, yy + 150 + (i - global.modsubmenuscroll) * 35, string_hash_to_newline(valueString));
            }}
        }}

        if (isSubmenu)
            draw_sprite(spr_heart, 0, _heartXPos, yy + 160 + ((global.modsubmenuno - global.modsubmenuscroll) * 35));
    }}
");

// Add menu step code
importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_darkcontroller_Step_0", "global.menucoord[0] = 4;", "global.menucoord[0] = array_length(global.modmenu_data) <= 0 ? 4 : 5;");
importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_darkcontroller_Step_0", "if (global.menucoord[0] == 4)", "if (global.menucoord[0] == (array_length(global.modmenu_data) <= 0 ? 4 : 5))");
importGroup.QueueAppend("gml_Object_obj_darkcontroller_Step_0", @$"
    if (global.menuno == 6)
    {{
        var isSubmenu = (global.modsubmenuno >= 0);

        if (!isSubmenu) {{
            // enter submenu right away if there is only one submenu
            if (array_length(global.modmenu_data) == 1)
                global.modsubmenuno = 0;

            if (left_p())
            {{
                movenoise = 1;

                global.modmenuno--;
                if (global.modmenuno < 0)
                    global.modmenuno = array_length(global.modmenu_data) - 1;
            }}
            if (right_p())
            {{
                movenoise = 1;

                global.modmenuno++;
                if (global.modmenuno >= array_length(global.modmenu_data))
                    global.modmenuno = 0;
            }}
            if (button1_p() && onebuffer < 0 && twobuffer < 0)
            {{
                onebuffer = 2;
                selectnoise = 1;
                global.modsubmenuno = 0;
            }}
            if (button2_p() && onebuffer < 0 && twobuffer < 0)
            {{
                cancelnoise = 1;
                twobuffer = 2;
                global.menuno = 0;
                global.submenu = 0;
            }}
        }} else if (!global.modsubmenuselected) {{
            var form_data = ds_map_find_value(global.modmenu_data[global.modmenuno], ""form"");
            var form_length = ds_map_exists(global.modmenu_data[global.modmenuno], ""form"") ? array_length(form_data) : 0;

            if (form_length <= 0) {{
                global.modsubmenuno = -1;
                global.modsubmenuscroll = 0;
            }}

            // back button
            form_length++;

            if (up_p())
            {{
                movenoise = 1;

                global.modsubmenuno--;

                if (global.modsubmenuno < global.modsubmenuscroll)
                    global.modsubmenuscroll = global.modsubmenuno;

                if (global.modsubmenuno < 0)
                {{
                    global.modsubmenuno = form_length - 1;
                    global.modsubmenuscroll = max(0, form_length - 7);
                }}
            }}
            if (down_p())
            {{
                movenoise = 1;

                global.modsubmenuno++;

                if (global.modsubmenuno >= global.modsubmenuscroll + 7)
                    global.modsubmenuscroll = global.modsubmenuno - 6;

                if (global.modsubmenuno >= form_length)
                {{
                    global.modsubmenuno = 0;
                    global.modsubmenuscroll = 0;
                }}
            }}
            if (button1_p() && onebuffer < 0 && twobuffer < 0)
            {{
                onebuffer = 2;
                selectnoise = 1;

                if (global.modsubmenuno >= array_length(form_data)) {{
                    global.modsubmenuno = -1;
                    global.modsubmenuscroll = 0;
                    
                    if (array_length(global.modmenu_data) == 1)
                    {{
                        global.menuno = 0;
                        global.submenu = 0;
                    }}
                }}
                else
                {{
                    global.modsubmenuselected = true;

                    // if range is only labels just cycle through them
                    var row_data = form_data[global.modsubmenuno];
                    var ranges = string_split({ds_map_find_value_lang("row_data", @"""value_range""")}, "";"");
                    var isAllLabels = true;

                    for (var i = 0; i < array_length(ranges); i++) {{
                        var range = ranges[i];
                        if (string_ends_with(range, ""%"")) {{
                            isAllLabels = false;
                            break;
                        }}
                    }}

                    var value = variable_instance_get(global, ds_map_find_value(row_data, ""value_name""));

                    if (isAllLabels) {{
                        global.modsubmenuselected = false;

                        for (var i = 0; i < array_length(ranges); i++) {{
                            var range = ranges[i];
                            if (string_pos(""="", range)) {{
                                var labelValue = string_split(range, ""="");
                                if (value < real(labelValue[1])) {{
                                    value = real(labelValue[1]);
                                    break;
                                }} else if (i+1 == array_length(ranges)) {{
                                    range = ranges[0];
                                    labelValue = string_split(range, ""="");
                                    value = real(labelValue[1]);
                                    break;
                                }}
                            }}
                        }}

                        variable_instance_set(global, ds_map_find_value(row_data, ""value_name""), value);
                    }}
                }}
            }}
            if (button2_p() && onebuffer < 0 && twobuffer < 0)
            {{
                cancelnoise = 1;
                twobuffer = 2;
                global.modsubmenuno = -1;
                global.modsubmenuscroll = 0;

                if (array_length(global.modmenu_data) == 1)
                {{
                    global.menuno = 0;
                    global.submenu = 0;
                }}
            }}
        }} else {{
            var form_data = ds_map_find_value(global.modmenu_data[global.modmenuno], ""form"");
            var row_data = form_data[global.modsubmenuno];
            var ranges = string_split({ds_map_find_value_lang("row_data", @"""value_range""")}, "";"");
            var value = variable_instance_get(global, ds_map_find_value(row_data, ""value_name""));
            
            if (right_h())
            {{
                if (value < 0.2)
                    value += 0.005;
                else if (value < 0.5)
                    value += 0.01;
                else if (value < 1)
                    value += 0.02;
                else if (value < 2)
                    value += 0.05;
                else
                    value += 0.1;

                for (var i = 0; i < array_length(ranges); i++) {{
                    var range = ranges[i];
                    if (string_ends_with(range, ""%"")) {{
                        var minMax = string_split(string_replace(range, ""%"", """"), ""-"");
                        if (value * 100 <= minMax[1] || i+1 == array_length(ranges)) {{
                            value = clamp(value, minMax[0] / 100, minMax[1] / 100);
                            break;
                        }}
                    }} else if (string_pos(""="", range)) {{
                        var labelValue = string_split(range, ""="");
                        if (value <= real(labelValue[1]) || i+1 == array_length(ranges)) {{
                            value = real(labelValue[1]);
                            break;
                        }}
                    }}
                }}

                variable_instance_set(global, ds_map_find_value(row_data, ""value_name""), value);
            }}
            
            if (left_h())
            {{
                if (value <= 0.2)
                    value -= 0.005;
                else if (value <= 0.5)
                    value -= 0.01;
                else if (value <= 1)
                    value -= 0.02;
                else if (value <= 2)
                    value -= 0.05;
                else
                    value -= 0.1;

                for (var i = array_length(ranges) - 1; i >= 0; i--) {{
                    var range = ranges[i];
                    if (string_ends_with(range, ""%"")) {{
                        var minMax = string_split(string_replace(range, ""%"", """"), ""-"");
                        if (value * 100 >= minMax[0] || i == 0) {{
                            value = clamp(value, minMax[0] / 100, minMax[1] / 100);
                            break;
                        }}
                    }} else if (string_pos(""="", range)) {{
                        var labelValue = string_split(range, ""="");
                        if (value >= real(labelValue[1]) || i == 0) {{
                            value = real(labelValue[1]);
                            break;
                        }}
                    }}
                }}

                variable_instance_set(global, ds_map_find_value(row_data, ""value_name""), value);
            }}
            
            se_select = 0;

            if (button1_p() && onebuffer < 0)
                se_select = 1;
            
            if (button2_p() && twobuffer < 0)
                se_select = 1;
            
            if (se_select == 1)
            {{
                selectnoise = 1;
                onebuffer = 2;
                twobuffer = 2;
                global.modsubmenuselected = false;
            }}
        }}
        
    }}
");

// Finish edit
importGroup.Import();
ScriptMessage($"Success: Mod menu added to '{displayName}'!");