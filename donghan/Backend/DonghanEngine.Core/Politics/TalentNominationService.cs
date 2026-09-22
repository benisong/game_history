using System;
using System.Collections.Generic;
using System.Linq;
using DonghanEngine.Core;

namespace DonghanEngine.Core.Politics;

/// <summary>
/// 纯领域服务：世家察举荐辟、天子御批任免与名士流向诸侯系统（单一职责）
/// </summary>
public sealed class TalentNominationService : ITalentNominationService
{
    private static readonly List<AristocratFamily> DefaultFamilies = new()
    {
        new AristocratFamily(
            FamilyId: "ru_nan_yuan",
            FamilyName: "汝南袁氏",
            NativeProvince: "yuzhou",
            Prestige: 95,
            Loyalty: 60,
            Power: 85,
            KeyFigureNpcIds: new[] { "yuan_shao", "yuan_shu" }),

        new AristocratFamily(
            FamilyId: "hong_nong_yang",
            FamilyName: "弘农杨氏",
            NativeProvince: "sili",
            Prestige: 90,
            Loyalty: 85,
            Power: 70,
            KeyFigureNpcIds: new[] { "yang_biao", "yang_xiu" }),

        new AristocratFamily(
            FamilyId: "ying_chuan_xun",
            FamilyName: "颍川荀氏",
            NativeProvince: "yuzhou",
            Prestige: 88,
            Loyalty: 80,
            Power: 65,
            KeyFigureNpcIds: new[] { "xun_yu", "xun_you" }),

        new AristocratFamily(
            FamilyId: "qiao_xian_cao",
            FamilyName: "谯县曹氏",
            NativeProvince: "yanzhou",
            Prestige: 75,
            Loyalty: 75,
            Power: 70,
            KeyFigureNpcIds: new[] { "cao_cao", "cao_ren" })
    };

    public IReadOnlyList<AristocratFamily> GetRegisteredFamilies() => DefaultFamilies.AsReadOnly();

    public IReadOnlyList<NominationCandidate> GenerateAnnualNominations(GameState state)
    {
        var candidates = new List<NominationCandidate>();

        // 汝南袁氏荐辟
        if (!state.Npcs.ContainsKey("xun_chen"))
        {
            candidates.Add(new NominationCandidate(
                CandidateId: "xun_chen",
                Name: "荀谌",
                SponsoringFamilyId: "ru_nan_yuan",
                NativeProvince: "yuzhou",
                Politics: 82,
                Intelligence: 85,
                Charisma: 78,
                Martial: 45,
                RecommendedOfficeTitle: "议郎",
                CandidateBio: "颍川名士，袁氏故吏，擅机辩长于谋略。"));
        }

        // 弘农杨氏荐辟
        if (!state.Npcs.ContainsKey("yang_xiu"))
        {
            candidates.Add(new NominationCandidate(
                CandidateId: "yang_xiu",
                Name: "杨修",
                SponsoringFamilyId: "hong_nong_yang",
                NativeProvince: "sili",
                Politics: 85,
                Intelligence: 92,
                Charisma: 80,
                Martial: 30,
                RecommendedOfficeTitle: "主簿",
                CandidateBio: "太尉杨彪之子，博学多才，聪敏过人。"));
        }

        // 颍川荀氏荐辟 (王佐之才荀彧)
        if (!state.Npcs.ContainsKey("xun_yu"))
        {
            candidates.Add(new NominationCandidate(
                CandidateId: "xun_yu",
                Name: "荀彧",
                SponsoringFamilyId: "ying_chuan_xun",
                NativeProvince: "yuzhou",
                Politics: 98,
                Intelligence: 96,
                Charisma: 94,
                Martial: 40,
                RecommendedOfficeTitle: "尚书令",
                CandidateBio: "字文若，有王佐之才，清秀通雅，海内名士之冠。"));
        }

        // 谯县曹氏荐辟
        if (!state.Npcs.ContainsKey("cao_ren"))
        {
            candidates.Add(new NominationCandidate(
                CandidateId: "cao_ren",
                Name: "曹仁",
                SponsoringFamilyId: "qiao_xian_cao",
                NativeProvince: "yanzhou",
                Politics: 60,
                Intelligence: 75,
                Charisma: 80,
                Martial: 90,
                RecommendedOfficeTitle: "别部司马",
                CandidateBio: "曹操从弟，严整知兵，勇略冠绝三军。"));
        }

        return candidates.AsReadOnly();
    }

    public NominationResolutionResult AppointCandidate(GameState state, NominationCandidate candidate, string officeTitle)
    {
        var family = DefaultFamilies.FirstOrDefault(f => f.FamilyId == candidate.SponsoringFamilyId);
        string familyName = family?.FamilyName ?? "名门世家";

        // 1. 在朝廷中注册登庸该 NPC
        var newNpc = new NpcState
        {
            Id = candidate.CandidateId,
            Name = candidate.Name,
            Title = officeTitle,
            TitleTier = 3,
            Favorability = 85, // 感念天子知遇之恩
            Power = candidate.Politics / 2,
            Corruption = 5,
            StashedWealth = 500,
            BirthYear = 163,
            BaseLongevity = 55,
            Personality = "忠诚",
            Style = "典雅",
            Faction = "清流派",
            Martial = candidate.Martial,
            Leadership = candidate.Martial,
            Politics = candidate.Politics,
            Charisma = candidate.Charisma,
            Ambition = 40,
            InitialLocation = "宣政殿",
            HistoricalRole = candidate.CandidateBio
        };
        state.RegisterNpc(newNpc);

        // 2. 提升该世家忠诚度，微增其在朝廷权势
        int familyLoyaltyDelta = 15;
        int familyPowerDelta = 10;
        int imperialPowerDelta = 3; // 广纳贤才提振皇权
        state.ImperialPower = Math.Clamp(state.ImperialPower + imperialPowerDelta, 0, 100);

        string title = $"【御批除官 · 门阀归心】天子擢拜{candidate.Name}为{officeTitle}！{familyName}阖族称颂！";
        string text = $"【察举】尚书台呈进孝廉名册，天子特旨御笔朱批：除拜{candidate.Name}为{officeTitle}。{familyName}感佩圣恩浩荡，上表誓忠王事，门阀向心力大增。";
        state.AddToChronicle(text);

        return new NominationResolutionResult(
            Appointed: true,
            CandidateId: candidate.CandidateId,
            CandidateName: candidate.Name,
            SponsoringFamilyId: candidate.SponsoringFamilyId,
            SponsoringFamilyLoyaltyDelta: familyLoyaltyDelta,
            SponsoringFamilyPowerDelta: familyPowerDelta,
            ImperialPowerDelta: imperialPowerDelta,
            DestinationFactionId: "court",
            NarrativeTitle: title,
            ChronicleText: text);
    }

    public NominationResolutionResult RejectCandidate(GameState state, NominationCandidate candidate)
    {
        var family = DefaultFamilies.FirstOrDefault(f => f.FamilyId == candidate.SponsoringFamilyId);
        string familyName = family?.FamilyName ?? "名门世家";

        // 1. 被驳回后，世家忠诚度下降
        int familyLoyaltyDelta = -20;
        int familyPowerDelta = -5;
        int imperialPowerDelta = 0;

        // 2. 判定人才外逃流向（根据家族地缘与名士特质）
        string destinationFactionId = candidate.SponsoringFamilyId switch
        {
            "ru_nan_yuan" => "yuan_shao",
            "ying_chuan_xun" => "cao_cao",
            "qiao_xian_cao" => "cao_cao",
            _ => "liu_bei"
        };

        string destinationName = destinationFactionId switch
        {
            "yuan_shao" => "渤海袁绍",
            "cao_cao" => "陈留曹操",
            "liu_bei" => "平原刘备",
            _ => "关东诸侯"
        };

        string title = $"【朝议弃才 · 名士外奔】天子驳回{candidate.Name}察举！名士心怀怨望投奔{destinationName}！";
        string text = $"【弃贤】天子以资历未充为由驳回{candidate.Name}之除官奏请。{familyName}心怀怨望，{candidate.Name}长叹而去，星夜出洛阳东门，投奔{destinationName}幕府！关东诸侯得此王佐贤佐，实力潜滋暗长。";
        state.AddToChronicle(text);

        return new NominationResolutionResult(
            Appointed: false,
            CandidateId: candidate.CandidateId,
            CandidateName: candidate.Name,
            SponsoringFamilyId: candidate.SponsoringFamilyId,
            SponsoringFamilyLoyaltyDelta: familyLoyaltyDelta,
            SponsoringFamilyPowerDelta: familyPowerDelta,
            ImperialPowerDelta: imperialPowerDelta,
            DestinationFactionId: destinationFactionId,
            NarrativeTitle: title,
            ChronicleText: text);
    }
}
