using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
ScriptMessage($"Adding custom difficulty to '{displayName}'...");

// Code edits
UndertaleModLib.Compiler.CodeImportGroup importGroup = new(Data){
    ThrowOnNoOpFindReplace = true
};

// Add globals
importGroup.QueueRegexFindReplace("gml_GlobalScript_scr_gamestart", "function scr_gamestart\\(\\)\\s*{", @$"
    function scr_gamestart()
    {{
        global.diff_damagemulti = 1;
        {(ch_no != 3 ? "" : @"
        global.diff_gameboarddmgx = -1;
        ")}
        global.diff_hitall = 0;
        global.diff_iframes = 1;
        global.diff_tpgain = 1;
        global.diff_battlerewards = 1;
        {(ch_no != 3 ? "" : @"
        global.diff_rewardranking = 0;
        ")}
        global.diff_downdeficit = 1 / 2;
        global.diff_downedregen = 1 / 8;
        global.diff_victoryres = 1 / 8;
        global.diff_resettodefaults = 0;

    ");
// Load globals from config
string[] loadLikes = {"gml_GlobalScript_scr_load"};
if (ch_no > 1)
{
    string[] loadCh1 = {"gml_GlobalScript_scr_load_chapter1"};
    loadLikes = loadLikes.Concat(loadCh1).ToArray();
}
if (ch_no > 2)
{
    string[] loadCh2 = {"gml_GlobalScript_scr_load_chapter2"};
    loadLikes = loadLikes.Concat(loadCh2).ToArray();
}
if (ch_no > 3)
{
    string[] loadCh3 = {"gml_GlobalScript_scr_load_chapter3"};
    loadLikes = loadLikes.Concat(loadCh3).ToArray();
}
foreach (string scrName in loadLikes)
{   
    importGroup.QueueTrimmedLinesFindReplace(scrName, "ossafe_file_text_close(myfileid);", @$"
        ossafe_file_text_close(myfileid);

        ossafe_ini_open(""difficulty_"" + string(global.filechoice) + "".ini"");
        global.diff_damagemulti = ini_read_real(""DIFFICULTY"", ""DAMAGE_MULTI"", 1);
        {(ch_no != 3 ? "" : @"
        global.diff_gameboarddmgx = ini_read_real(""DIFFICULTY"", ""GAMEBOARD_DMG_X"", -1);
        ")}
        global.diff_hitall = ini_read_real(""DIFFICULTY"", ""HIT_ALL"", 0);
        global.diff_iframes = ini_read_real(""DIFFICULTY"", ""I_FRAMES"", 1);
        global.diff_tpgain = ini_read_real(""DIFFICULTY"", ""TP_GAIN"", 1);
        global.diff_battlerewards = ini_read_real(""DIFFICULTY"", ""BATTLE_REWARDS"", 1);
        {(ch_no != 3 ? "" : @"
        global.diff_rewardranking = ini_read_real(""DIFFICULTY"", ""REWARD_RANKING"", 0);
        ")}
        global.diff_downdeficit = ini_read_real(""DIFFICULTY"", ""DOWN_DEFICIT"", 1 / 2);
        global.diff_downedregen = ini_read_real(""DIFFICULTY"", ""DOWNED_REGEN"", 1 / 8);
        global.diff_victoryres = ini_read_real(""DIFFICULTY"", ""VICTORY_RES"", 1 / 8);
        ossafe_ini_close();

        ");
}
// Save globals to config
importGroup.QueueTrimmedLinesFindReplace("gml_GlobalScript_scr_saveprocess", "ossafe_file_text_close(myfileid);", @$"
    ossafe_file_text_close(myfileid);

    ossafe_ini_open(""difficulty_"" + string(global.filechoice) + "".ini"");
    ini_write_real(""DIFFICULTY"", ""DAMAGE_MULTI"", global.diff_damagemulti);
    {(ch_no != 3 ? "" : @"
    ini_write_real(""DIFFICULTY"", ""GAMEBOARD_DMG_X"", global.diff_gameboarddmgx);
    ")}
    ini_write_real(""DIFFICULTY"", ""HIT_ALL"", global.diff_hitall);
    ini_write_real(""DIFFICULTY"", ""I_FRAMES"", global.diff_iframes);
    ini_write_real(""DIFFICULTY"", ""TP_GAIN"", global.diff_tpgain);
    ini_write_real(""DIFFICULTY"", ""BATTLE_REWARDS"", global.diff_battlerewards);
    {(ch_no != 3 ? "" : @"
    ini_write_real(""DIFFICULTY"", ""REWARD_RANKING"", global.diff_rewardranking);
    ")}
    ini_write_real(""DIFFICULTY"", ""DOWN_DEFICIT"", global.diff_downdeficit);
    ini_write_real(""DIFFICULTY"", ""DOWNED_REGEN"", global.diff_downedregen);
    ini_write_real(""DIFFICULTY"", ""VICTORY_RES"", global.diff_victoryres);
    ossafe_ini_close();
    ");

// Add mod menu
importGroup.QueueAppend("gml_Object_obj_darkcontroller_Create_0", @$"
    
    if (!variable_instance_exists(global, ""modmenu_data""))
        global.modmenu_data = array_create(0);

    var menudata = ds_map_create();
    ds_map_add(menudata, ""title_en"", ""Difficulty"");

    var formdata = array_create(0);

    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""Damage Multi"");
    ds_map_add(rowdata, ""value_range_en"", ""0-1000%;INF=2147483647"");
    ds_map_add(rowdata, ""value_name"", ""diff_damagemulti"");
    array_push(formdata, rowdata);

    {(ch_no != 3 ? "" : @"
    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""Gameboard Dmg X"");
    ds_map_add(rowdata, ""value_range_en"", ""INHERIT=-1;0-1000%;INF=2147483647"");
    ds_map_add(rowdata, ""value_name"", ""diff_gameboarddmgx"");
    array_push(formdata, rowdata);
    ")}

    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""Hit.All"");
    ds_map_add(rowdata, ""value_range_en"", ""OFF=0;ON=1"");
    ds_map_add(rowdata, ""value_name"", ""diff_hitall"");
    array_push(formdata, rowdata);

    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""I-Frames"");
    ds_map_add(rowdata, ""value_range_en"", ""0-1000%"");
    ds_map_add(rowdata, ""value_name"", ""diff_iframes"");
    array_push(formdata, rowdata);

    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""TP Gain"");
    ds_map_add(rowdata, ""value_range_en"", ""0-1000%"");
    ds_map_add(rowdata, ""value_name"", ""diff_tpgain"");
    array_push(formdata, rowdata);

    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""Battle Rewards"");
    ds_map_add(rowdata, ""value_range_en"", ""0-1000%"");
    ds_map_add(rowdata, ""value_name"", ""diff_battlerewards"");
    array_push(formdata, rowdata);

    {(ch_no != 3 ? "" : @"
    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""Reward Ranking"");
    ds_map_add(rowdata, ""value_range_en"", ""OFF=0;ON=1"");
    ds_map_add(rowdata, ""value_name"", ""diff_rewardranking"");
    array_push(formdata, rowdata);
    ")}

    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""Down Deficit"");
    ds_map_add(rowdata, ""value_range_en"", ""0-1000%;[-999]=2147483647"");
    ds_map_add(rowdata, ""value_name"", ""diff_downdeficit"");
    array_push(formdata, rowdata);

    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""Downed Regen"");
    ds_map_add(rowdata, ""value_range_en"", ""0-1000%;INSTANT=2147483647"");
    ds_map_add(rowdata, ""value_name"", ""diff_downedregen"");
    array_push(formdata, rowdata);

    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""Victory Res"");
    ds_map_add(rowdata, ""value_range_en"", ""OFF=-1;0-100%"");
    ds_map_add(rowdata, ""value_name"", ""diff_victoryres"");
    array_push(formdata, rowdata);

    // TODO requires a refactor to modmenu but would be much better to be able to have menu buttons trigger a function, this is a hacky work-a-round
    var rowdata = ds_map_create();
    ds_map_add(rowdata, ""title_en"", ""Reset to Defaults"");
    ds_map_add(rowdata, ""value_range_en"", ""=0;=1"");
    ds_map_add(rowdata, ""value_name"", ""diff_resettodefaults"");
    array_push(formdata, rowdata);

    ds_map_add(menudata, ""form"", formdata);

    array_push(global.modmenu_data, menudata);
");

importGroup.QueueAppend("gml_Object_obj_darkcontroller_Step_0", @$"
    if (global.diff_resettodefaults > 0)
    {{
        global.diff_damagemulti = 1;
        {(ch_no != 3 ? "" : @"
        global.diff_gameboarddmgx = -1;
        ")}
        global.diff_hitall = 0;
        global.diff_iframes = 1;
        global.diff_tpgain = 1;
        global.diff_battlerewards = 1;
        {(ch_no != 3 ? "" : @"
        global.diff_rewardranking = 0;
        ")}
        global.diff_downdeficit = 1 / 2;
        global.diff_downedregen = 1 / 8;
        global.diff_victoryres = 1 / 8;
        global.diff_resettodefaults = 0;
    }}
");

string[] damageLikes = {"gml_GlobalScript_scr_damage"};
if (ch_no >= 2)
{
    string[] ch2UpDamageLikes = {"gml_GlobalScript_scr_damage_proportional", "gml_GlobalScript_scr_damage_sneo_final_attack"};
    damageLikes = damageLikes.Concat(ch2UpDamageLikes).ToArray();
}
if (ch_no == 3)
{
    string[] ch3DamageLikes = {"gml_GlobalScript_scr_damage_fixed", "gml_GlobalScript_scr_damage_maxhp"};
    damageLikes = damageLikes.Concat(ch3DamageLikes).ToArray();
}

// Apply damage multiplier
foreach (string scrName in damageLikes)
{   
    importGroup.QueueTrimmedLinesFindReplace(scrName, "hpdiff = tdamage;", @"
        origdamage = tdamage;
        tdamage = ceil(tdamage * global.diff_damagemulti);
        hpdiff = tdamage;
        ");
    importGroup.QueueRegexFindReplace(scrName, "if \\(target == 3\\)\\s*{", @"
        if (target == 3)
        {
            tdamage = origdamage;
        ");
    importGroup.QueueTrimmedLinesFindReplace(scrName, "if (global.charaction[hpi] == 10)", @"
        tdamage = ceil(tdamage * global.diff_damagemulti);

        if (global.charaction[hpi] == 10)
        ");
}
importGroup.QueueTrimmedLinesFindReplace("gml_GlobalScript_scr_damage_all_overworld", "hpdiff = tdamage;", @"
    tdamage = ceil(tdamage * global.diff_damagemulti);
    hpdiff = tdamage;
    ");
if (ch_no == 1)
{
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_laserscythe_Other_15", "global.hp[global.char[i]] = ceil(global.hp[global.char[i]] * 0.7);",
        "global.hp[global.char[i]] = ceil(global.hp[global.char[i]] * power(0.7, global.diff_damagemulti));");
}
if (ch_no == 2)
{
    importGroup.QueueFindReplace("gml_Object_o_boxingcontroller_Collision_o_boxing_hitbox", "if (global.hp[1] <= 10)", "if (global.hp[1] <= round(10 * global.diff_damagemulti))");
    importGroup.QueueFindReplace("gml_Object_o_boxingcontroller_Collision_o_boxing_hitbox", "global.hp[1] -= 10;", "global.hp[1] -= round(10 * global.diff_damagemulti);");
    importGroup.QueueFindReplace("gml_Object_o_boxingcontroller_Collision_o_boxing_hitbox", "global.hp[1] / 2", "(global.diff_damagemulti != 0 ? (global.hp[1] / power(2, 1 / global.diff_damagemulti)) : 0)");
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_o_boxingcontroller_Collision_o_boxing_hitbox", "global.hp[1] -= final_damage_amount;", 
        "global.hp[1] -= final_damage_amount * global.diff_damagemulti;");
}
if (ch_no == 3)
{
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_quizsequence_Other_13", "var _damage = irandom_range(30, 38);",
        "var _damage = ceil(irandom_range(30, 38) * global.diff_damagemulti);");
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_ch3_b4_chef_kris_Create_0", "global.hp[1] = clamp(global.hp[1] - damage_amount, 1, global.maxhp[1]);", @"
        damage_amount = ceil(damage_amount * global.diff_damagemulti);

        global.hp[1] = clamp(global.hp[1] - damage_amount, 1, global.maxhp[1]);
    ");
}
if (ch_no == 4) {
    string[] sandbagLimits = {"tdamage", "hpdiff"};
    foreach (string limit in sandbagLimits)
    {
        importGroup.QueueTrimmedLinesFindReplace("gml_GlobalScript_scr_damage", $"if (global.chapter == 4 && i_ex(obj_hammer_of_justice_enemy) && {limit} < 5)", 
            $"if (global.chapter == 4 && i_ex(obj_hammer_of_justice_enemy) && {limit} < (5 * global.diff_damagemulti))");
        importGroup.QueueTrimmedLinesFindReplace("gml_GlobalScript_scr_damage", $"{limit} = 5;", $"{limit} = 5 * global.diff_damagemulti;");
    }
}
// Apply damage multiplier (Damage Over Time)
if (ch_no >= 2)
{
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_battlecontroller_Step_0", "t_siner++;", "");
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_battlecontroller_Step_0", "if (global.charweapon[4] == 13)", @"
        if (global.charweapon[4] == 13)
        {
            if (global.hp[4] > round(global.maxhp[4] / 3))
                global.hp[4] = max(round(global.maxhp[4] / 3), global.hp[4] - floor(t_siner / 6));
            
            t_siner = t_siner % 6;
            t_siner += global.diff_damagemulti;
        }

        if (false)
    ");
    string poisonScrName = ch_no == 2 ? "gml_Object_obj_heroparent_Draw_0" : "gml_Object_obj_heroparent_Step_0";
    importGroup.QueueTrimmedLinesFindReplace(poisonScrName, "poisontimer++;", @"
        poisontimer++;
        poisondmgtimer += global.diff_damagemulti;
    ");
    importGroup.QueueTrimmedLinesFindReplace(poisonScrName, "global.hp[global.char[myself]]--;", 
        "global.hp[global.char[myself]] = max(1, global.hp[global.char[myself]] - floor(poisondmgtimer / 10));");
    importGroup.QueueTrimmedLinesFindReplace(poisonScrName, "poisonamount = 0;", @"
        poisonamount = 0;
        poisondmgtimer = 0;
    ");
    importGroup.QueueTrimmedLinesFindReplace(poisonScrName, "poisontimer = 0;", @"
        poisontimer = 0;
        poisondmgtimer = poisondmgtimer % 10;
    ");
}
if (ch_no == 4)
{
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_incense_cloud_Other_15", "repeat (_r)", @"
        _r = ceil(_r * global.diff_damagemulti);
        repeat (_r)
    ");
}

// Apply BattleBoard damage multiplier
if (ch_no == 3)
{
    const string batboarddmgmulti = "(global.diff_gameboarddmgx < 0 ? global.diff_damagemulti : global.diff_gameboarddmgx)";
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_board_puzzlebombbullet_Step_0", "myhealth -= other.damage;",
        $"myhealth -= other.damage * {batboarddmgmulti};");
    importGroup.QueueFindReplace("gml_Object_obj_quizsequence_Draw_0", "myhealth -= 2;",
        $"myhealth -= 2 * {batboarddmgmulti};");
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_b1rocks2_Step_0", "ralsei.myhealth -= 1;",
        $"ralsei.myhealth = max(1, ralsei.myhealth - {batboarddmgmulti});");
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_mainchara_board_Step_0", "myhealth -= hazard.damage;",
        $"myhealth -= hazard.damage * {batboarddmgmulti};");
    string[] hangInThereRalsei = {"gml_Object_obj_b1rocks1_Step_0", "gml_Object_obj_b1lancer_Step_0", "gml_Object_obj_b3bridge_Step_0", "gml_Object_obj_b1power_Step_0"};
    foreach (string scrName in hangInThereRalsei)
    {   
        importGroup.QueueTrimmedLinesFindReplace(scrName, "myhealth--;", $"myhealth = max(1, myhealth - {batboarddmgmulti});");
    }
    // fix health item not working between 0 and 1
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_board_heal_pickup_Step_0", "if (obj_board_controller.kris_object.myhealth < 1)",
        "if (obj_board_controller.kris_object.myhealth <= 0)");
}

// Apply down penalty
foreach (string scrName in damageLikes)
{   
    importGroup.QueueFindReplace(scrName, "global.maxhp[chartarget] / 2", "max(-999, global.maxhp[chartarget] * global.diff_downdeficit)");
    importGroup.QueueFindReplace(scrName, "global.maxhp[0] / 2", "max(-999, global.maxhp[0] * global.diff_downdeficit)");
}
if (ch_no == 4) {
    importGroup.QueueFindReplace("gml_GlobalScript_scr_down_partymember", "global.maxhp[_chartarget] / 2", "max(-999, global.maxhp[_chartarget] * global.diff_downdeficit)");
    string[] heavySmokers = {"1", "2", "3"};
    foreach (string smoker in heavySmokers)
    {
        importGroup.QueueFindReplace("gml_Object_obj_incense_cloud_Other_15", $"global.maxhp[{smoker}] / 2", $"max(-999, global.maxhp[{smoker}] * global.diff_downdeficit)");
    }
}

// Apply victory res - if VictoryRes is 0 then don't heal; additionally ensure the heal brings the character to at least 1 hp for low values of VictoryRes
importGroup.QueueFindReplace("gml_Object_obj_battlecontroller_Step_0", "global.maxhp[i] / 8", "global.diff_victoryres >= 0 ? max(1, global.maxhp[i] * global.diff_victoryres) : global.hp[i]");

// Downed Regen
importGroup.QueueTrimmedLinesFindReplace("gml_GlobalScript_scr_mnendturn", "healamt = ceil(global.maxhp[hptarget] / 8);", "healamt = ceil(global.maxhp[hptarget] * global.diff_downedregen);");

// Hit.All
string[] singleHits = {"gml_Object_obj_overworldbulletparent_Other_15", "gml_Object_obj_collidebullet_Other_15", "gml_Object_obj_checkers_leap_Other_15"};
if (ch_no == 1) {
    string[] ch1SingleHits = {"gml_Object_obj_regularbullet_permanent_Other_15", "gml_Object_obj_lancerbike_Other_15", "gml_Object_obj_lancerbike_neo_Other_15"};
    singleHits = singleHits.Concat(ch1SingleHits).ToArray();
}
if (ch_no == 2) {
    string[] ch2SingleHits = {"gml_Object_obj_mettaton_bomb_hitbox_Other_15", "gml_Object_obj_lancerbike_Other_15", "gml_Object_obj_baseenemy_Step_0", "gml_Object_obj_omawaroid_vaccine_Other_15",
        "gml_Object_obj_viro_needle_Other_15", "gml_Object_obj_yarnbullet_Other_15", "gml_Object_obj_tasque_soundwave_Other_15", "gml_Object_obj_tm_quizzap_Other_15",
        "gml_Object_obj_queen_social_media_Other_15", "gml_Object_obj_queen_wine_attack_droplet_Other_15", "gml_Object_obj_queen_wine_attack_bottom_hurtbox_Other_15",
        "gml_Object_obj_queen_winebubble_Other_15", "gml_Object_obj_sneo_elevator_electric_ball_Other_15", "gml_Object_obj_shrinktangle_Step_0", "gml_Object_obj_sneo_phonehand_master_hurtbox_Other_15",
        "gml_Object_obj_thrash_duck_bullet_Other_15"};
    singleHits = singleHits.Concat(ch2SingleHits).ToArray();
}
if (ch_no == 3) {
    string[] ch3SingleHits = {"gml_GlobalScript_scr_damage_manual", "gml_Object_obj_bullet_dice_Other_15", "gml_Object_obj_knight_roaring_star_Other_15", "gml_Object_obj_susiezilla_statue_Alarm_0",
        "gml_Object_obj_knight_weird_circle_bullet_Other_15", "gml_Object_obj_tenna_enemy_Step_0", "gml_Object_obj_tenna_enemy_Step_0", "gml_Object_obj_regularbullet_elnina_Other_15",
        "gml_Object_obj_knight_diamondswordbullet_ext_Other_15", "gml_Object_obj_rouxls_yarnball_Other_15", "gml_Object_obj_elnina_snowring_Other_15", "gml_Object_obj_knight_enemy_Other_12",
        "gml_Object_obj_snowflake_ult_bullet_Other_15", "gml_Object_obj_rouxls_biplane_flag_Other_15", "gml_Object_obj_knight_pointing_starchild_Other_15",
        "gml_Object_obj_knight_pointing_starchild_Other_15", "gml_Object_obj_precipitation_bullet_parent_Other_15", "gml_Object_obj_bullet_rain_Other_15", "gml_Object_obj_elnina_raindrop_Other_15",
        "gml_Object_obj_bullet_sun_Other_15", "gml_Object_obj_rouxls_helicopter_hitbox_Other_15", "gml_Object_obj_tenna_allstars_bullet_Other_15", "gml_Object_obj_elnina_bouncingbullet_Other_15",
        "gml_Object_obj_tm_quizzap_Other_15", "gml_Object_obj_bullet_snow_Other_15", "gml_Object_obj_rainwater_Other_15", "gml_Object_obj_bullet_homing_Other_15",
        "gml_Object_obj_knight_pointing_star_Other_15", "gml_Object_obj_lanino_solar_system_Other_15", "gml_Object_obj_yarnsnake_bullet_Other_15", "gml_Object_obj_knight_bullethell_bullet2_Other_15",
        "gml_Object_obj_bullet_moon_Other_15", "gml_Object_obj_watercooler_enemy_Other_12", "gml_Object_obj_roaringknight_slash_Other_15", "gml_Object_obj_bullet_submoon_Other_15"};
    singleHits = singleHits.Concat(ch3SingleHits).ToArray();
}
if (ch_no == 4) {
    string[] ch4SingleHits = {"gml_Object_obj_incense_bullet_Other_15", "gml_Object_obj_climb_kris_Step_0", "gml_Object_obj_rotating_object_parent_new_Other_15",
        "gml_Object_obj_holywater_act_line_Other_15", "gml_Object_obj_yarnsnake_bullet_Other_15", "gml_Object_obj_gerson_shell_pinball_Other_15", "gml_Object_obj_regularbullet_elnina_Other_15",
        "gml_Object_obj_spearshot_Other_10", "gml_Object_obj_spearshot_Other_10", "gml_Object_obj_gerson_hammer_bro_hammer_Other_15", "gml_Object_obj_darkness_bullet_Other_15",
        "gml_Object_obj_organ_enemy_vertical_pillar_Other_15", "gml_Object_obj_mizzle_spotlight_eye_Other_15", "gml_Object_obj_holywatercooler_enemy_Other_12",
        "gml_Object_obj_ghosthouse_jackolantern_merciful_Other_15", "gml_Object_obj_gerson_hammer_bounce_left_Other_15", "gml_Object_obj_overworld_knight_sword2_Other_15",
        "gml_Object_obj_darkshape_bigblast_Other_15", "gml_Object_obj_overworld_knight_sword1_Other_15", "gml_Object_obj_mike_hairball_Other_15",
        "gml_Object_obj_ghosthouse_jackolantern_merciful_old_Other_15", "gml_Object_obj_vertical_dark_shockwave_hurtbox_Other_15", "gml_Object_obj_bullet_dice_Other_15",
        "gml_Object_obj_gerson_swing_down_Other_15", "gml_Object_obj_giant_hammer_Step_0", "gml_Object_obj_gerson_hammer_bounce_down_Other_15", "gml_Object_obj_gerson_growtangle_telegraph_new_Other_15",
        "gml_Object_obj_gh_fireball_bouncy_Other_15", "gml_Object_obj_incense_bullet_fire_Other_15", "gml_Object_obj_tm_quizzap_Other_15", "gml_Object_obj_ow_pathingenemy_Other_15",
        "gml_Object_obj_lightbullet_Other_15", "gml_Object_obj_elnina_bouncingbullet_Other_15", "gml_Object_obj_gh_fireball_linear_Other_15", "gml_Object_obj_mike_spike_Other_15"};
    singleHits = singleHits.Concat(ch4SingleHits).ToArray();
}
foreach (string scrName in singleHits)
{
    importGroup.QueueTrimmedLinesFindReplace(scrName, "scr_damage();", @"
        {
            if (global.diff_hitall <= 0)
            {
                scr_damage();
            }
            else
            {
                scr_damage_all();
            }
        }
    ");
}
if (ch_no == 2) {
    // TODO might be cleaner to add a scr_damage_all_proportional function
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_basicbullet_sneo_finale_Other_15", "if (target != 3)", @"
        if (target != 3 && global.diff_hitall > 0)
        {
            if (global.inv < 0)
            {
                scr_damage_cache();
                remdamage = damage;
                _temptarget = target;

                for (ti = 0; ti < 3; ti += 1)
                {
                    global.inv = -1;
                    damage = remdamage;
                    target = ti;

                    if (global.hp[global.char[ti]] > 0 && global.char[ti] != 0)
                        scr_damage_proportional();
                }

                global.inv = global.invc * 40;
                target = _temptarget;
                scr_damage_check();
            }
        }

        if (target != 3 && global.diff_hitall <= 0)
    ");
}
if (ch_no == 3) {
    // TODO might be cleaner to add a scr_damage_all_maxhp function
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_roaringknight_splitslash_Step_0", "if (target != 3)", @"
        if (target != 3 && global.diff_hitall > 0)
        {
            if (global.inv < 0)
            {
                scr_damage_cache();
                remdamage = damage;
                _temptarget = target;

                for (ti = 0; ti < 3; ti += 1)
                {
                    global.inv = -1;
                    damage = remdamage;
                    target = ti;

                    if (global.hp[global.char[ti]] > 0 && global.char[ti] != 0)
                        scr_damage_maxhp(0.66, false, true);
                }

                global.inv = global.invc * 40;
                target = _temptarget;
                scr_damage_check();
            }
        }

        if (target != 3 && global.diff_hitall <= 0)
    ");
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_roaringknight_quickslash_big_Step_0", "if (target != 3)", @"
        if (target != 3 && global.diff_hitall > 0)
        {
            if (global.inv < 0)
            {
                scr_damage_cache();
                remdamage = damage;
                _temptarget = target;

                for (ti = 0; ti < 3; ti += 1)
                {
                    global.inv = -1;
                    damage = remdamage;
                    target = ti;

                    if (global.hp[global.char[ti]] > 0 && global.char[ti] != 0)
                        scr_damage_maxhp(1.25);
                }

                global.inv = global.invc * 40;
                target = _temptarget;
                scr_damage_check();
            }
        }

        if (target != 3 && global.diff_hitall <= 0)
    ");
}

// I-Frames
string[] iFramers = {"gml_GlobalScript_scr_damage", "gml_GlobalScript_scr_damage_all", "gml_GlobalScript_scr_damage_all_overworld"};
if (ch_no == 1) {
    string[] ch1IFramers = {"gml_Object_obj_laserscythe_Other_15"};
    iFramers = iFramers.Concat(ch1IFramers).ToArray();
}
if (ch_no == 2) {
    string[] ch2IFramers = {"gml_Object_obj_basicbullet_sneo_finale_Other_15", "gml_Object_obj_spamton_neo_enemy_Other_12"};
    iFramers = iFramers.Concat(ch2IFramers).ToArray();
    importGroup.QueueFindReplace("gml_Object_obj_sneo_fakeheart_Create_0", "global.inv = 300", "global.inv = global.diff_iframes * 300");
}
if (ch_no >= 2) {
    string[] ch2to4IFramers = {"gml_GlobalScript_scr_damage_sneo_final_attack", "gml_GlobalScript_scr_damage_proportional", "gml_GlobalScript_scr_weaken_party"};
    iFramers = iFramers.Concat(ch2to4IFramers).ToArray();
}
if (ch_no == 3) {
    string[] ch3IFramers = {"gml_Object_obj_roaringknight_splitslash_Step_0", "gml_Object_obj_roaringknight_quickslash_big_Step_0", "gml_GlobalScript_scr_damage_fixed",
    "gml_GlobalScript_scr_damage_maxhp", "gml_Object_obj_tenna_enemy_Step_0", "gml_Object_obj_knight_enemy_Other_12", "gml_Object_obj_watercooler_enemy_Other_12"};
    iFramers = iFramers.Concat(ch3IFramers).ToArray();
    importGroup.QueueFindReplace("gml_Object_obj_tenna_zoom_Step_0", "global.inv = 30", "global.inv = global.diff_iframes * 30");
}
if (ch_no == 4) {
    string[] ch4IFramers = {"gml_Object_obj_mike_raindrop_Other_15", "gml_Object_obj_holywatercooler_enemy_Other_12"};
    iFramers = iFramers.Concat(ch4IFramers).ToArray();
    importGroup.QueueFindReplace("gml_Object_obj_gerson_fakeheart_Create_0", "global.inv = 300", "global.inv = global.diff_iframes * 300");
    importGroup.QueueFindReplace("gml_Object_obj_sound_of_justice_enemy_Step_0", "global.inv = 29", "global.inv = global.diff_iframes * 29");
    importGroup.QueueFindReplace("gml_Object_obj_ow_pathingenemy_Other_15", "global.inv = 60", "global.inv = global.diff_iframes * 60");
    importGroup.QueueFindReplace("gml_Object_obj_hammer_of_justice_enemy_Step_0", "global.inv = 19", "global.inv = global.diff_iframes * 19");
    importGroup.QueueFindReplace("gml_Object_obj_small_jackolantern_Other_15", "global.inv = min(global.inv, 10)", "global.inv = min(global.inv, global.diff_iframes * 10)");
    importGroup.QueueFindReplace("gml_Object_obj_ghosthouse_jackolantern_Other_15", "global.inv = min(global.inv, 10 - floor(hits / 2))",
        "global.inv = min(global.inv, global.diff_iframes * (10 - floor(hits / 2)))");
}
foreach (string scrName in iFramers)
{
    importGroup.QueueFindReplace(scrName, "global.inv = global.invc", "global.inv = global.diff_iframes * global.invc");
}

// Apply Battle Rewards
importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_battlecontroller_Step_0", "global.xp += global.monsterexp[3];", @"
    global.monsterexp[3] *= global.diff_battlerewards;
    global.xp += global.monsterexp[3];
");
importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_battlecontroller_Step_0", "global.gold += global.monstergold[3];", @"
    global.monstergold[3] *= global.diff_battlerewards;
    global.gold += global.monstergold[3];
");
if (ch_no == 3) {
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_gameshow_battlemanager_Step_0", " var scoretoAdd = totalstring;", @"
        var scoretoAdd = totalstring;
        scoretoAdd = string(global.diff_battlerewards * real(scoretoAdd));
    ");
}

// Apply Reward Ranking
if (ch_no == 3) {
    importGroup.QueueTrimmedLinesFindReplace("gml_Object_obj_gameshow_battlemanager_Draw_0", "global.flag[1116] += real(totalstring);", @"
        global.flag[1116] += global.diff_rewardranking > 0 ? (global.diff_battlerewards * real(totalstring)) : real(totalstring);
    ");
}

// Apply TP Gain
string[] tensionHeals = {"gml_Object_obj_grazebox_Collision_obj_collidebullet"};
if (ch_no >= 1 && ch_no <= 2) {
    string[] ch1to2TensionHeals = {"gml_Object_obj_heroparent_Alarm_1"};
    tensionHeals = tensionHeals.Concat(ch1to2TensionHeals).ToArray();
}
if (ch_no == 2) {
    string[] ch2TensionHeals = {"gml_Object_obj_sneo_lilguy_Collision_obj_yheart_shot", "gml_Object_obj_sneo_crusher_Collision_obj_yheart_shot",
        "gml_Object_obj_pipis_egg_bullet_Collision_obj_yheart_shot", "gml_Object_obj_pipis_egg_bullet_Collision_obj_mettaton_bomb_hitbox", "gml_Object_obj_rouxls_enemy_Create_0",
        "gml_Object_o_boxingcontroller_Step_0", "gml_Object_o_boxinggraze_Alarm_0"};
    tensionHeals = tensionHeals.Concat(ch2TensionHeals).ToArray();
}
if (ch_no >= 3 && ch_no <= 4) {
    string[] ch3to4TensionHeals = {"gml_Object_obj_heroparent_Step_0"};
    tensionHeals = tensionHeals.Concat(ch3to4TensionHeals).ToArray();
}
if (ch_no == 3) {
    string[] ch3TensionHeals = {"gml_Object_obj_tracking_sword_slash_extra_graze_Step_0", "gml_Object_obj_heroparent_Other_10"};
    tensionHeals = tensionHeals.Concat(ch3TensionHeals).ToArray();
}
if (ch_no == 4) {
    string[] ch4TensionHeals = {"gml_GlobalScript_scr_boltcheck", "gml_Object_obj_ghosthouse_key_Other_15", "gml_Object_obj_spearshot_Other_10", "gml_Object_obj_ghosthouse_dot_Other_15",
        "gml_Object_obj_darkshape_greenblob_Step_0", "gml_Object_obj_attackpress_Other_11", "gml_Object_obj_hammer_of_justice_enemy_Draw_0"};
    tensionHeals = tensionHeals.Concat(ch4TensionHeals).ToArray();
}
foreach (string scrName in tensionHeals)
{
    importGroup.QueueFindReplace(scrName, "scr_tensionheal(", "scr_tensionheal(global.diff_tpgain * ");
}
// avoid tp heal items
if (ch_no == 4) {
    importGroup.QueueFindReplace("gml_Object_obj_battlecontroller_Step_0", "scr_tensionheal(5", "scr_tensionheal(global.diff_tpgain * 5");
}
importGroup.QueueFindReplace("gml_Object_obj_battlecontroller_Step_0", "scr_tensionheal(40", "scr_tensionheal(global.diff_tpgain * 40");

// Finish edit
importGroup.Import();
ScriptMessage($"Success: Custom difficulty added to '{displayName}'!");