using System;
using System.Collections.Generic;

namespace DonghanEngine.Core.Geopolitics.Models;

public enum WarlordPosture
{
    LoyalistBanner,     // 忠贞藩屏（恪守臣节、听调平叛）
    AmbitiousWarlord,   // 称霸枭雄（自主攻伐、战后讨封）
    CautiousAutonomist  // 自守宗室（保境安民、纳贡求安）
}

public enum CampaignStatus
{
    Mobilizing, // 誓师出征
    Sieging,    // 围城攻防
    Stalemate,  // 关隘对峙
    Victory,    // 攻方大捷夺地
    Defeated    // 攻方溃退
}

public enum WarlordActionType
{
    Idle,           // 休养生息/屯田
    Tribute,        // 上缴朝贡
    LaunchCampaign, // 出兵攻伐
    PetitionRank,   // 战后上表讨封
    AppealForHelp   // 告急求援
}
