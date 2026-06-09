using System.Collections.Generic;

namespace DonghanEngine.Core;

public static class HistoricalNpcRelations
{
    public static List<NpcRelation> All { get; } = new()
    {
        R("he_jin", "he_miao", NpcRelationType.Kinship, 78, "外戚同党", "何氏外戚集团核心人物"),
        R("he_jin", "zhang_rang", NpcRelationType.Hostility, 88, "诛宦死敌", "何进与十常侍围绕诛宦、宫禁军权对立"),
        R("he_jin", "yuan_shao", NpcRelationType.FactionAlly, 68, "诛宦暂盟", "袁绍依附外戚诛宦路线，后引发宫变升级"),
        R("zhang_rang", "zhao_zhong", NpcRelationType.FactionAlly, 92, "十常侍核心", "张让、赵忠同为灵帝宠宦与中常侍核心"),
        R("zhang_rang", "duan_gui", NpcRelationType.FactionAlly, 76, "十常侍同党", "十常侍宫禁网络"),
        R("zhang_rang", "bi_lan", NpcRelationType.FactionAlly, 72, "十常侍同党", "十常侍宫禁网络"),
        R("zhang_rang", "xia_yun", NpcRelationType.FactionAlly, 72, "十常侍同党", "十常侍宫禁网络"),
        R("zhang_rang", "guo_sheng", NpcRelationType.FactionAlly, 70, "十常侍同党", "十常侍宫禁网络"),
        R("zhang_rang", "song_dian", NpcRelationType.FactionAlly, 66, "十常侍同党", "十常侍宫禁网络"),
        R("zhang_rang", "han_kui", NpcRelationType.FactionAlly, 66, "十常侍同党", "十常侍宫禁网络"),
        R("jian_shuo", "zhang_rang", NpcRelationType.Rivalry, 54, "宫中军权竞争", "同属宦官系统，但蹇硕掌西园军，和内廷宠宦有权力竞争"),

        R("yuan_wei", "yuan_shao", NpcRelationType.Kinship, 90, "袁氏宗族", "袁隗为袁氏宗族长辈，袁绍依托四世三公门第"),
        R("yuan_wei", "yuan_shu", NpcRelationType.Kinship, 90, "袁氏宗族", "袁隗为袁氏宗族长辈，袁术为袁氏嫡支权贵"),
        R("yuan_shao", "yuan_shu", NpcRelationType.Rivalry, 72, "宗族相竞", "同出袁氏而政治路线与继承声望相争"),
        R("yuan_wei", "yang_biao", NpcRelationType.FactionAlly, 55, "门阀清议", "袁氏、弘农杨氏皆为汉末门阀政治重要节点"),

        R("lu_zhi", "liu_bei", NpcRelationType.TeacherStudent, 86, "师生", "刘备曾从卢植学"),
        R("lu_zhi", "gongsun_zan", NpcRelationType.TeacherStudent, 80, "师生", "公孙瓒亦为卢植门下"),
        R("qiao_xuan", "cao_cao", NpcRelationType.Patronage, 78, "赏识提携", "桥玄以知人闻名，曾赏识曹操"),
        R("xun_shuang", "xun_yu", NpcRelationType.Kinship, 84, "颍川荀氏", "荀爽、荀彧同出颍川荀氏"),
        R("xun_shuang", "xun_you", NpcRelationType.Kinship, 82, "颍川荀氏", "荀爽、荀攸同出颍川荀氏"),
        R("xun_yu", "xun_you", NpcRelationType.Kinship, 88, "荀氏宗族", "荀彧、荀攸同族并为后期重要谋臣"),
        R("xun_yu", "guo_jia", NpcRelationType.RegionalTie, 68, "颍川士人", "颍川士人网络与后期曹魏谋士圈"),
        R("xun_yu", "cheng_yu", NpcRelationType.FactionAlly, 58, "谋臣网络", "后期曹魏政略圈，可作为年月触发后的士人联系"),

        R("ding_yuan", "lu_bu", NpcRelationType.Command, 82, "并州上下", "吕布早期依附丁原并州军系统"),
        R("dong_zhuo", "li_jue", NpcRelationType.Command, 86, "董卓部曲", "李傕为董卓集团部将"),
        R("dong_zhuo", "guo_si", NpcRelationType.Command, 84, "董卓部曲", "郭汜为董卓集团部将"),
        R("dong_zhuo", "zhang_ji", NpcRelationType.Command, 74, "凉州军系", "张济属凉州军阀网络"),
        R("dong_zhuo", "jia_xu", NpcRelationType.FactionAlly, 62, "凉州谋士", "贾诩出凉州体系，后与董卓余部相关"),
        R("zhang_ji", "zhang_xiu", NpcRelationType.Kinship, 92, "叔侄军系", "张绣为张济从子，承其部众"),
        R("ma_teng", "han_sui", NpcRelationType.Rivalry, 76, "凉州同盟相竞", "马腾、韩遂在凉州时盟时争"),
        R("bian_zhang", "beigong_boyu", NpcRelationType.FactionAlly, 80, "凉州叛乱同盟", "凉州叛乱中汉吏与羌胡首领合兵"),
        R("bian_zhang", "han_sui", NpcRelationType.FactionAlly, 72, "凉州叛乱网络", "韩遂与边章同属凉州叛乱政治网络"),

        R("liu_bei", "guan_yu", NpcRelationType.SwornBond, 96, "义从同心", "刘备、关羽为义兵核心同伴"),
        R("liu_bei", "zhang_fei", NpcRelationType.SwornBond, 96, "义从同心", "刘备、张飞为义兵核心同伴"),
        R("guan_yu", "zhang_fei", NpcRelationType.SwornBond, 90, "并肩战友", "关羽、张飞同为刘备核心武臣"),
        R("liu_bei", "zhao_yun", NpcRelationType.Patronage, 58, "后续投契", "赵云与刘备关系适合后续事件触发，不宜开局强绑定"),
    };

    private static NpcRelation R(string from, string to, NpcRelationType type, int strength, string label, string basis, bool mutual = true)
    {
        return new NpcRelation
        {
            FromNpcId = from,
            ToNpcId = to,
            Type = type,
            Strength = strength,
            Label = label,
            HistoricalBasis = basis,
            IsMutual = mutual,
        };
    }
}
