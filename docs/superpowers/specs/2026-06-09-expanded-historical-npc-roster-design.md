# 2026-06-09 东汉末年汉灵帝：群臣 NPC 扩充设计稿

> **状态：📋 设计阶段 — 未实现**
>
> 本文档用于扩充《东汉末年汉灵帝》NPC 池。目标不是一次性把所有人物塞进开局朝堂，而是建立“开局在朝 + 地方可调 + 冷备登场 + 乱世势力”的群臣生态。

## 1. 设计目标

当前游戏开局可见 NPC 偏少，朝会、黄门密札、地方任免、平叛与西园军务的候选人重复度较高。新增 NPC 应满足：

- **史实感**：优先取自《后汉书》《三国志》及《三国演义》传统人物体系；演义人物不直接覆盖正史定位，而作为叙事色彩补强。
- **玩法分工**：每人要能服务至少一种玩法：朝会党争、赈灾民政、州郡治理、军事平叛、抄家风险、AI 辩论、后续割据。
- **分批登场**：不是所有人都在 184 年洛阳朝堂；分为“开局在朝 / 地方任官 / 冷备可召 / 乱世触发 / 反叛敌对”。
- **数据顺序统一**：每个人按“人物基础数据 → 生平 → 游戏数据”书写，方便之后直接转成 JSON 或 C# preset。

## 2. 字段模板

### 2.1 人物基础数据

- ID：snake_case，稳定唯一。
- 姓名：中文姓名。
- 字：若不详则写“不详”。
- 生卒：年份；不确定则用“约”。
- 籍贯：郡县/州。
- 史料定位：后汉书/三国志/演义中的主要身份。
- 开局位置：洛阳朝堂、地方州郡、在野、边军、敌对势力。
- 初始官职：贴近 184 年前后，不强行给未来官职。

### 2.2 生平

- 早年与入仕。
- 中平年间或灵帝末年关键事迹。
- 与黄巾、外戚、宦官、清流、地方军阀的关系。
- 演义差异：若演义形象明显影响玩家认知，单独标注。

### 2.3 游戏数据

- Faction：清流派 / 外戚派 / 阉党派 / 西园亲军 / 地方州牧 / 割据军阀 / 在野名士 / 反叛势力。
- TitleTier：0-4。
- BirthYear / BaseLongevity。
- Favorability：对汉灵帝初始好感。
- Power：朝堂/地方政治权势。
- Corruption：贪腐度。
- StashedWealth：可抄没私蓄，单位万钱。
- 五维：Martial / Leadership / Politics / Charisma / Ambition。
- Traits：优先使用现有 `TraitNames`，必要时列“建议新增 Trait”。
- Personality / Style。
- 推荐用途：太守、平叛、招安、朝会发言、抄家目标、剧情触发等。
- 登场规则：开局、指定年月、事件触发、州郡危机触发。

## 3. 登场层级建议

### 3.1 开局在朝 / 洛阳高频互动

这些人应尽快进入玩家视野，服务朝会、抄家、党争、奏折。

- 何进、张让、蹇硕、曹操：已在开局。
- 建议新增开局在朝：袁绍、袁术、王允、卢植、朱儁、皇甫嵩、赵忠、段珪、毕岚、何苗、杨彪、马日磾、蔡邕。

### 3.2 地方任官 / 可召入京

这些人用于黄门密札、任命太守、地方风险、后续割据。

- 董卓、刘焉、刘虞、丁原、韩馥、刘表、陶谦、孔融、乔瑁、张邈、鲍信、公孙瓒、孙坚、公孙度、马腾、韩遂。

### 3.3 青年冷备 / 未来名臣名将

年龄偏小或尚未显达，不宜开局坐满朝堂，但可通过举荐、征辟、战事触发。

- 刘备、关羽、张飞、荀彧、荀攸、郭嘉、程昱、贾诩、张辽、吕布、张郃、沮授、田丰。

### 3.4 敌对或半敌对势力

主要用于危机，不一定作为可任命臣子。

- 张角、张宝、张梁、张燕、张牛角、边章、北宫伯玉。

## 4. 第一批重点群臣设计（建议先落地 32 人）

### 4.1 袁绍

**人物基础数据**

- ID：`yuan_shao`
- 姓名：袁绍，字本初
- 生卒：154-202
- 籍贯：汝南汝阳
- 史料定位：四世三公袁氏子弟，灵帝末年西园八校尉之一，后为河北强藩。
- 开局位置：洛阳朝堂 / 西园军系统
- 初始官职：中军校尉或虎贲中郎将相关序列

**生平**

袁绍出身汝南袁氏，门生故吏遍布天下。灵帝末年参与洛阳政治，既与清流士人相近，又有门阀结党基础。何进谋诛宦官时，袁绍是重要推动者。董卓入洛后逃出京师，后来据冀州，成为北方最强诸侯之一。演义中强调其“外宽内忌”“多谋少断”。

**游戏数据**

- Faction：清流派 / 门阀世家
- TitleTier：2
- BirthYear：154；BaseLongevity：48
- Favorability：42；Power：35；Corruption：25；StashedWealth：800
- 五维：Martial 55 / Leadership 68 / Politics 62 / Charisma 86 / Ambition 86
- Traits：`MenFaShiJia`, `ChuShenMingMen`, `ShouXiaYouBing`, `LaoMouShenSuan`
- Personality：外宽内忌
- Style：结党营私
- 推荐用途：朝会门阀代表、平衡何进、地方任命高风险候选、未来冀州割据触发源。
- 登场规则：开局在朝或西园成立后自动在朝。

### 4.2 袁术

**人物基础数据**

- ID：`yuan_shu`
- 姓名：袁术，字公路
- 生卒：约155-199
- 籍贯：汝南汝阳
- 史料定位：袁氏嫡支权贵，后据淮南，僭号称帝。
- 开局位置：洛阳朝堂 / 门阀圈
- 初始官职：虎贲中郎将、折冲校尉类

**生平**

袁术与袁绍同出袁氏，但嫡庶名分与政治路线相冲。灵帝末年在洛阳权贵圈活动，依靠家世轻视寒门与地方武人。董卓乱后南下，后据淮南，因称帝而众叛亲离。演义中被塑造成骄矜、奢侈、短视的门阀野心家。

**游戏数据**

- Faction：清流派 / 门阀世家
- TitleTier：2
- BirthYear：155；BaseLongevity：44
- Favorability：35；Power：30；Corruption：55；StashedWealth：1200
- 五维：Martial 45 / Leadership 52 / Politics 45 / Charisma 60 / Ambition 92
- Traits：`MenFaShiJia`, `ChuShenMingMen`, `HaoSheWuDu`, `TanDeWuYan`
- Personality：骄矜
- Style：结党营私
- 推荐用途：可抄家但高反噬；任地方官会增加割据风险；朝会与袁绍互相拆台。
- 登场规则：开局在朝；若被外放到富庶州郡， Ambition 触发地方称雄事件。

### 4.3 王允

**人物基础数据**

- ID：`wang_yun`
- 姓名：王允，字子师
- 生卒：137-192
- 籍贯：太原祁县
- 史料定位：东汉末清流重臣，后主持诛董。
- 开局位置：洛阳朝堂
- 初始官职：尚书令 / 侍中候补

**生平**

王允早年以刚正知名，曾因得罪宦官和权贵而沉浮。董卓专权后，他隐忍周旋，最终联络吕布诛杀董卓。正史中的王允能力强而性格峻急，诛董后不能妥善安抚凉州集团，导致李傕郭汜反攻长安。演义中借貂蝉连环计使其形象更具谋略色彩。

**游戏数据**

- Faction：清流派
- TitleTier：3
- BirthYear：137；BaseLongevity：55
- Favorability：48；Power：40；Corruption：8；StashedWealth：120
- 五维：Martial 20 / Leadership 45 / Politics 82 / Charisma 72 / Ambition 58
- Traits：`GangZhiBuE`, `JingTianWeiDi`, `LaoMouShenSuan`, `QingZhengLianJie`
- Personality：刚峻
- Style：雷厉风行
- 推荐用途：抄家钦差、朝会反宦官、诛董剧情关键人；过度重用会压迫军阀派。
- 登场规则：开局在朝。

### 4.4 卢植

**人物基础数据**

- ID：`lu_zhi`
- 姓名：卢植，字子干
- 生卒：约139-192
- 籍贯：涿郡涿县
- 史料定位：经学名儒、名将，刘备师长，黄巾初期统兵。
- 开局位置：洛阳 / 黄巾前线
- 初始官职：北中郎将

**生平**

卢植学问深厚，门生众多。黄巾之乱爆发后奉命讨张角，围困广宗，后因不肯贿赂宦官左丰而被诬陷下狱。其经历非常适合表现“清流有才但受宦官掣肘”的主题。演义也保留其为刘备师长与讨黄巾名将形象。

**游戏数据**

- Faction：清流派
- TitleTier：3
- BirthYear：139；BaseLongevity：53
- Favorability：55；Power：28；Corruption：3；StashedWealth：80
- 五维：Martial 55 / Leadership 82 / Politics 80 / Charisma 76 / Ambition 18
- Traits：`ZhiJunYanZheng`, `ShanChangMinZheng`, `GangZhiBuE`, `QingZhengLianJie`
- Personality：刚正
- Style：直言守礼
- 推荐用途：黄巾平叛高可靠主将、招安使臣、清流朝议；若阉党权势高，容易被构陷。
- 登场规则：开局在朝；黄巾爆发时自动进入平叛候选。

### 4.5 皇甫嵩

**人物基础数据**

- ID：`huangfu_song`
- 姓名：皇甫嵩，字义真
- 生卒：约130-195
- 籍贯：安定朝那
- 史料定位：东汉末平黄巾名将。
- 开局位置：洛阳 / 前线
- 初始官职：左中郎将、后左车骑将军

**生平**

皇甫嵩是平定黄巾的核心将领之一，战功卓著，治军严整。相较董卓，他更像“可依赖但不专横”的帝国军人。其忠于汉室、野心不高，适合作为玩家早期挽救天下的关键武力资源。

**游戏数据**

- Faction：清流派 / 汉室军方
- TitleTier：3
- BirthYear：130；BaseLongevity：60
- Favorability：60；Power：32；Corruption：10；StashedWealth：200
- 五维：Martial 76 / Leadership 93 / Politics 56 / Charisma 72 / Ambition 22
- Traits：`ZhiJunYanZheng`, `AiBingRuZi`, `GangZhiBuE`, `DongDianBingFa`
- Personality：忠勇
- Style：雷厉风行
- 推荐用途：平叛王牌；可稳定军心；不适合抄家党争。
- 登场规则：开局在朝或黄巾爆发后立即可用。

### 4.6 朱儁

**人物基础数据**

- ID：`zhu_jun`
- 姓名：朱儁，字公伟
- 生卒：？-195
- 籍贯：会稽上虞
- 史料定位：平黄巾名将，后任太尉。
- 开局位置：洛阳 / 前线
- 初始官职：右中郎将

**生平**

朱儁以军功显名，与皇甫嵩共同参与平定黄巾。相比皇甫嵩，朱儁更具地方军政经验，适合南方、豫州、荆州方向平叛。董卓乱政后仍维护汉室名义，晚年卷入李傕郭汜乱局。

**游戏数据**

- Faction：清流派 / 汉室军方
- TitleTier：3
- BirthYear：135；BaseLongevity：60
- Favorability：56；Power：26；Corruption：12；StashedWealth：180
- 五维：Martial 72 / Leadership 86 / Politics 58 / Charisma 66 / Ambition 28
- Traits：`ZhiJunYanZheng`, `DongDianBingFa`, `TiXuShiZu`
- Personality：沉毅
- Style：务实平乱
- 推荐用途：平叛副王牌、地方太守、黄巾战报核心。
- 登场规则：开局在朝或黄巾爆发后登场。

### 4.7 赵忠

**人物基础数据**

- ID：`zhao_zhong`
- 姓名：赵忠
- 生卒：？-189
- 籍贯：不详
- 史料定位：十常侍之一，灵帝宠宦。
- 开局位置：洛阳宫中
- 初始官职：中常侍

**生平**

赵忠是灵帝朝权势宦官，与张让等同掌内廷。灵帝曾称“张常侍是我父，赵常侍是我母”，足见其亲近。宦官集团把持诏令、卖官鬻爵、干预司法，是外戚与清流共同忌恨的对象。

**游戏数据**

- Faction：阉党派
- TitleTier：3
- BirthYear：135；BaseLongevity：55
- Favorability：72；Power：68；Corruption：88；StashedWealth：5200
- 五维：Martial 8 / Leadership 12 / Politics 58 / Charisma 62 / Ambition 82
- Traits：`TanDeWuYan`, `ChanMeiZhuanQuan`, `HuiPaiMaPi`
- Personality：谄媚阴狠
- Style：谄媚专权
- 推荐用途：高额抄家目标、高风险阉党保护伞、卖官剧情关键人。
- 登场规则：开局在朝。

### 4.8 段珪

**人物基础数据**

- ID：`duan_gui`
- 姓名：段珪
- 生卒：？-189
- 籍贯：不详
- 史料定位：十常侍之一。
- 开局位置：洛阳宫中
- 初始官职：中常侍

**生平**

段珪为十常侍成员，灵帝末年宦官集团核心人物之一。何进被杀、少帝出奔等洛阳政变中，宦官集团的末路与其相关。相比张让、赵忠，段珪可作为阉党中层代表，增加阉党内部层次。

**游戏数据**

- Faction：阉党派
- TitleTier：2
- BirthYear：140；BaseLongevity：50
- Favorability：66；Power：45；Corruption：78；StashedWealth：2600
- 五维：Martial 10 / Leadership 12 / Politics 45 / Charisma 48 / Ambition 72
- Traits：`TanDeWuYan`, `HuiPaiMaPi`, `YouXieXinJi`
- Personality：机警
- Style：结党护短
- 推荐用途：阉党中层、被抄家后牵出张让赵忠、宫变事件节点。
- 登场规则：开局在朝。

### 4.9 毕岚

**人物基础数据**

- ID：`bi_lan`
- 姓名：毕岚
- 生卒：不详
- 籍贯：不详
- 史料定位：灵帝宠宦，以工程、苑囿、宫室相关事见载。
- 开局位置：洛阳宫中
- 初始官职：中常侍

**生平**

毕岚可代表灵帝朝宫廷工程与享乐消费线。他不像张让赵忠那样只服务党争，也能服务“修苑囿、开渠、宫室、鬻官筹款”等内廷玩法。史料中灵帝营造、卖官、内库相关弊政可由其承载。

**游戏数据**

- Faction：阉党派
- TitleTier：2
- BirthYear：142；BaseLongevity：52
- Favorability：70；Power：38；Corruption：82；StashedWealth：1800
- 五维：Martial 5 / Leadership 10 / Politics 42 / Charisma 55 / Ambition 65
- Traits：`PuZhangLangFei`, `TanDeWuYan`, `ChanMeiZhuanQuan`
- Personality：巧佞
- Style：宫室营造
- 推荐用途：西园/私库/工程花费事件；抄家收益中高；赈灾漂没严重。
- 登场规则：开局在朝。

### 4.10 何苗

**人物基础数据**

- ID：`he_miao`
- 姓名：何苗
- 生卒：？-189
- 籍贯：南阳宛
- 史料定位：何皇后异父兄，外戚集团成员。
- 开局位置：洛阳朝堂
- 初始官职：车骑将军相关

**生平**

何苗是何进外戚集团的一员，与何进共同构成外戚权力。其能力与声望不如何进，但能代表外戚内部的利益分赃、军权分配和宫廷亲属政治。何进遇害后，外戚集团瓦解，何苗亦死于乱局。

**游戏数据**

- Faction：外戚派
- TitleTier：3
- BirthYear：140；BaseLongevity：49
- Favorability：38；Power：50；Corruption：52；StashedWealth：1100
- 五维：Martial 42 / Leadership 40 / Politics 35 / Charisma 38 / Ambition 66
- Traits：`ShouXiaYouBing`, `YouXieShouZang`, `ChuShenMingMen`
- Personality：依附权势
- Style：外戚分权
- 推荐用途：外戚派二号人物、何进被削弱后的替补、抄家中风险目标。
- 登场规则：开局在朝。

### 4.11 杨彪

**人物基础数据**

- ID：`yang_biao`
- 姓名：杨彪，字文先
- 生卒：142-225
- 籍贯：弘农华阴
- 史料定位：弘农杨氏名臣，汉末三公之一，杨修之父。
- 开局位置：洛阳朝堂
- 初始官职：侍中 / 尚书

**生平**

杨彪出身弘农杨氏，门第显赫，历仕汉末多朝。其政治生涯贯穿灵帝、献帝与曹操时期，常作为汉室旧臣和门阀清议的代表。相比袁氏更稳健，适合作为高门但不极端割据的朝臣。

**游戏数据**

- Faction：清流派 / 门阀世家
- TitleTier：3
- BirthYear：142；BaseLongevity：83
- Favorability：50；Power：38；Corruption：18；StashedWealth：600
- 五维：Martial 15 / Leadership 35 / Politics 78 / Charisma 75 / Ambition 35
- Traits：`MenFaShiJia`, `ChuShenMingMen`, `ShanChangMinZheng`
- Personality：持重
- Style：守礼周旋
- 推荐用途：朝会稳健派、任太守较安全、缓冲袁氏门阀压力。
- 登场规则：开局在朝。

### 4.12 马日磾

**人物基础数据**

- ID：`ma_ridi`
- 姓名：马日磾，字翁叔
- 生卒：？-194
- 籍贯：扶风茂陵
- 史料定位：经学名臣，后汉末太傅。
- 开局位置：洛阳朝堂
- 初始官职：谏议大夫 / 侍中

**生平**

马日磾是经学与礼制型重臣，汉末常作为朝廷名义与礼法秩序的象征。他不适合军事平叛，却适合处理诏令合法性、安抚士人、修复清议。后期出使关东，受袁术轻慢，体现中央名义衰败。

**游戏数据**

- Faction：清流派
- TitleTier：3
- BirthYear：140；BaseLongevity：54
- Favorability：54；Power：32；Corruption：6；StashedWealth：150
- 五维：Martial 8 / Leadership 20 / Politics 76 / Charisma 72 / Ambition 20
- Traits：`QingZhengLianJie`, `ShanChangMinZheng`, `QinMinWenHe`
- Personality：温厚守礼
- Style：礼法调停
- 推荐用途：招安、朝会合法性、降低清流不满；不适合强硬处置。
- 登场规则：开局在朝。

### 4.13 蔡邕

**人物基础数据**

- ID：`cai_yong`
- 姓名：蔡邕，字伯喈
- 生卒：132-192
- 籍贯：陈留圉
- 史料定位：文学家、经学家，蔡文姬之父。
- 开局位置：洛阳/贬谪边缘，可召还
- 初始官职：议郎、待诏文士

**生平**

蔡邕以文章、书法、经学闻名，曾因直言和党争受牵连。董卓入洛后强征蔡邕入朝并礼遇，后因感念董卓而被王允下狱死。其适合作为“文化声望、清议、士人舆论”的 NPC，而非数值型强臣。

**游戏数据**

- Faction：清流派 / 在野名士
- TitleTier：1
- BirthYear：132；BaseLongevity：60
- Favorability：45；Power：18；Corruption：2；StashedWealth：60
- 五维：Martial 5 / Leadership 15 / Politics 62 / Charisma 82 / Ambition 12
- Traits：`QingZhengLianJie`, `XiHaoQingTan`, `QinMinWenHe`
- Personality：文雅直敏
- Style：清议文名
- 推荐用途：降低士人怨气、修史制礼、文化事件；若朝局残酷易被牵连。
- 登场规则：开局可召还；清流派被打压时可能下野。

### 4.14 董卓

**人物基础数据**

- ID：`dong_zhuo`
- 姓名：董卓，字仲颖
- 生卒：约139-192
- 籍贯：陇西临洮
- 史料定位：凉州军阀，后入洛专权。
- 开局位置：并州/河东/凉州边军
- 初始官职：并州刺史、河东太守相关

**生平**

董卓久在西北边地，与羌胡战争、凉州军队联系深厚。黄巾乱起时可被朝廷征调，但其军队更忠于个人而非中央。何进召外兵入京最终引董卓进入洛阳，废立皇帝，挟天子以令群臣，是汉末秩序崩坏的标志。

**游戏数据**

- Faction：割据军阀
- TitleTier：3
- BirthYear：139；BaseLongevity：53
- Favorability：25；Power：35；Corruption：70；StashedWealth：900
- 五维：Martial 86 / Leadership 78 / Politics 42 / Charisma 52 / Ambition 96
- Traits：`KongWuYouLi`, `YongBingZiZhong`, `ShouXiaYouBing`, `TanDeWuYan`
- Personality：残暴豪强
- Style：拥兵自重
- 推荐用途：高战力平叛但极高入京风险；可作为“召外兵”灾难性选择。
- 登场规则：地方冷备；黄巾或宫变时可召，若带兵入洛触发董卓专权链。

### 4.15 丁原

**人物基础数据**

- ID：`ding_yuan`
- 姓名：丁原，字建阳
- 生卒：？-189
- 籍贯：泰山南城或并州相关任官
- 史料定位：并州刺史，吕布义父/上官。
- 开局位置：并州边军
- 初始官职：并州刺史

**生平**

丁原掌并州兵，与吕布关系密切。董卓入洛后，丁原曾与其对峙，后被吕布所杀。作为 NPC，他代表“非董卓的边军选择”：也有兵权，但政治手腕与个人魅力不如董卓，风险较低但不稳定。

**游戏数据**

- Faction：地方州牧 / 汉室军方
- TitleTier：3
- BirthYear：140；BaseLongevity：50
- Favorability：45；Power：32；Corruption：25；StashedWealth：350
- 五维：Martial 62 / Leadership 68 / Politics 36 / Charisma 45 / Ambition 58
- Traits：`ShouXiaYouBing`, `DongDianBingFa`, `YouXieLiQi`
- Personality：粗直
- Style：边军自守
- 推荐用途：并州平叛、牵制董卓；若吕布在其麾下，存在被策反风险。
- 登场规则：并州局势恶化或召边军时登场。

### 4.16 吕布

**人物基础数据**

- ID：`lu_bu`
- 姓名：吕布，字奉先
- 生卒：？-199
- 籍贯：五原九原
- 史料定位：汉末猛将，先后依附丁原、董卓等。
- 开局位置：并州边军
- 初始官职：主簿/骑将

**生平**

吕布以骁勇闻名，正史与演义均强调其武力卓绝、反复无常。早期依附丁原，后受董卓诱使杀丁原，再拜董卓为义父，后又杀董卓。其适合作为游戏里的“最高武力、最低忠诚稳定性”角色。

**游戏数据**

- Faction：西北边军 / 割据军阀
- TitleTier：1
- BirthYear：156；BaseLongevity：43
- Favorability：35；Power：18；Corruption：35；StashedWealth：200
- 五维：Martial 98 / Leadership 72 / Politics 18 / Charisma 60 / Ambition 82
- Traits：`KongWuYouLi`, `YouXieLiQi`, `ShouXiaYouBing`
- 建议新增 Trait：`反复无常`（受贿/高压/低好感时叛变概率上升）
- Personality：骁勇躁动
- Style：逐利易主
- 推荐用途：个人战斗王牌；不宜长期外任；可触发丁原/董卓/王允连环事件。
- 登场规则：丁原或董卓登场后作为附属将领出现。

### 4.17 刘焉

**人物基础数据**

- ID：`liu_yan`
- 姓名：刘焉，字君郎
- 生卒：？-194
- 籍贯：江夏竟陵
- 史料定位：汉室宗亲，益州牧，州牧制度关键人物。
- 开局位置：洛阳朝堂 / 地方候任
- 初始官职：宗正、太常类

**生平**

刘焉是汉室宗亲，见天下将乱，建议设置州牧以重地方权力，自己出任益州牧，后形成割据基础。其是游戏中“中央为救火而放权地方，结果催生军阀”的核心制度人物。

**游戏数据**

- Faction：地方州牧 / 宗室
- TitleTier：3
- BirthYear：140；BaseLongevity：54
- Favorability：42；Power：34；Corruption：28；StashedWealth：500
- 五维：Martial 20 / Leadership 50 / Politics 78 / Charisma 70 / Ambition 78
- Traits：`LaoMouShenSuan`, `ShanChangMinZheng`, `ChuShenMingMen`
- Personality：深谋自保
- Style：外镇自立
- 推荐用途：州牧制度提案；任命远州能稳定地方但埋下割据。
- 登场规则：地方叛乱蔓延或朝会讨论州牧制度时登场。

### 4.18 刘虞

**人物基础数据**

- ID：`liu_yu`
- 姓名：刘虞，字伯安
- 生卒：？-193
- 籍贯：东海郯
- 史料定位：汉室宗亲，幽州牧，以仁政安边。
- 开局位置：幽州/北方边地
- 初始官职：幽州刺史或候任

**生平**

刘虞以宽仁治边著称，能安抚胡汉百姓。与公孙瓒路线相反：刘虞主张怀柔，公孙瓒主张强硬军事。两人冲突代表边疆政策选择。作为 NPC，他是低野心、高民政、高招安的地方官模板。

**游戏数据**

- Faction：地方州牧 / 宗室
- TitleTier：3
- BirthYear：140；BaseLongevity：53
- Favorability：58；Power：30；Corruption：5；StashedWealth：120
- 五维：Martial 18 / Leadership 48 / Politics 82 / Charisma 84 / Ambition 16
- Traits：`AiMinRuZi`, `QinMinWenHe`, `QingZhengLianJie`, `ShanChangMinZheng`
- Personality：宽厚
- Style：怀柔安边
- 推荐用途：太守/州牧最佳民政人选、招安特使；军事威慑不足。
- 登场规则：北方边患或幽州事件触发。

### 4.19 刘表

**人物基础数据**

- ID：`liu_biao`
- 姓名：刘表，字景升
- 生卒：142-208
- 籍贯：山阳高平
- 史料定位：八俊之一，后为荆州牧。
- 开局位置：洛阳清流 / 地方候任
- 初始官职：北军中候、清流名士

**生平**

刘表是汉末名士，后单骑入荆州，联合当地豪族稳定州境。其能力在于政治整合与士人声望，而非进取统一。游戏中适合作为“能稳地方但会形成温和割据”的州牧型 NPC。

**游戏数据**

- Faction：清流派 / 地方州牧
- TitleTier：2
- BirthYear：142；BaseLongevity：66
- Favorability：50；Power：24；Corruption：20；StashedWealth：300
- 五维：Martial 20 / Leadership 58 / Politics 78 / Charisma 80 / Ambition 55
- Traits：`ShanChangMinZheng`, `ChuShenMingMen`, `XiHaoQingTan`
- Personality：儒雅守成
- Style：联合士族
- 推荐用途：荆州治理、士人安抚；长期外任会降低中央控制。
- 登场规则：荆州动荡或清流举荐。

### 4.20 陶谦

**人物基础数据**

- ID：`tao_qian`
- 姓名：陶谦，字恭祖
- 生卒：132-194
- 籍贯：丹阳
- 史料定位：徐州牧，汉末地方长官。
- 开局位置：地方州郡
- 初始官职：议郎 / 地方官候补

**生平**

陶谦在汉末任徐州牧，形象介于地方实力派与守成官僚之间。正史中其治理与用人评价复杂，演义中则常以让徐州给刘备的仁厚老臣形象出现。游戏中可作为地方稳定者，但并非高能力名臣。

**游戏数据**

- Faction：地方州牧
- TitleTier：2
- BirthYear：132；BaseLongevity：62
- Favorability：48；Power：25；Corruption：32；StashedWealth：400
- 五维：Martial 30 / Leadership 54 / Politics 58 / Charisma 62 / Ambition 42
- Traits：`QinMinWenHe`, `YouXieShouZang`
- Personality：守成
- Style：地方自保
- 推荐用途：中等太守、徐州事件、刘备登场引线。
- 登场规则：地方官池冷备。

### 4.21 孔融

**人物基础数据**

- ID：`kong_rong`
- 姓名：孔融，字文举
- 生卒：153-208
- 籍贯：鲁国
- 史料定位：孔子后裔、名士，后为北海相。
- 开局位置：洛阳/在野名士
- 初始官职：司徒掾、侍御史类

**生平**

孔融以名士风骨、文章和清议著称，政治能力不如声望耀眼。后任北海相，面对黄巾与地方军阀时颇为吃力。曹操当权后因言论获罪。游戏中他应是高声望、低军事、易引发清议事件的人物。

**游戏数据**

- Faction：清流派 / 在野名士
- TitleTier：1
- BirthYear：153；BaseLongevity：55
- Favorability：46；Power：20；Corruption：3；StashedWealth：80
- 五维：Martial 8 / Leadership 28 / Politics 60 / Charisma 86 / Ambition 22
- Traits：`XiHaoQingTan`, `QingZhengLianJie`, `ShuoHuaZhiLv`, `ChuShenMingMen`
- Personality：傲直
- Style：清议讥弹
- 推荐用途：提高士人支持；朝会直言会冲撞阉党/权臣；不宜平叛。
- 登场规则：清流举荐或北海危机。

### 4.22 公孙瓒

**人物基础数据**

- ID：`gongsun_zan`
- 姓名：公孙瓒，字伯圭
- 生卒：？-199
- 籍贯：辽西令支
- 史料定位：幽州边将，白马义从首领。
- 开局位置：幽州边地
- 初始官职：涿县令 / 骑都尉候补

**生平**

公孙瓒长期与北方胡族作战，以强硬军事著称。与刘虞怀柔路线冲突，后据幽州，与袁绍争雄。游戏中他应是强平叛、低民政、高边军自主性的角色。

**游戏数据**

- Faction：割据军阀 / 边军
- TitleTier：2
- BirthYear：153；BaseLongevity：46
- Favorability：40；Power：24；Corruption：30；StashedWealth：300
- 五维：Martial 82 / Leadership 74 / Politics 30 / Charisma 56 / Ambition 68
- Traits：`KongWuYouLi`, `DongDianBingFa`, `ShouXiaYouBing`
- Personality：刚烈
- Style：强硬安边
- 推荐用途：北方平叛、骑兵事件；与刘虞同州会冲突。
- 登场规则：幽州/冀州危机触发。

### 4.23 孙坚

**人物基础数据**

- ID：`sun_jian`
- 姓名：孙坚，字文台
- 生卒：155-191
- 籍贯：吴郡富春
- 史料定位：江东猛将，长沙太守，破虏将军。
- 开局位置：地方军官
- 初始官职：佐军司马 / 县令经历

**生平**

孙坚以勇烈善战闻名，参与平定黄巾、讨董卓等战事，是江东孙氏兴起的奠基者。其忠汉色彩与个人武力并重，但死得较早，适合成为高战力、短寿命、地方势力种子。

**游戏数据**

- Faction：地方军方 / 割据军阀潜质
- TitleTier：2
- BirthYear：155；BaseLongevity：37
- Favorability：52；Power：18；Corruption：18；StashedWealth：180
- 五维：Martial 90 / Leadership 82 / Politics 36 / Charisma 66 / Ambition 70
- Traits：`KongWuYouLi`, `ZhiJunYanZheng`, `GangZhiBuE`
- Personality：勇烈
- Style：先登破敌
- 推荐用途：南方平叛王牌；若长期外任会开启江东孙氏线。
- 登场规则：黄巾南线或长沙事件。

### 4.24 刘备

**人物基础数据**

- ID：`liu_bei`
- 姓名：刘备，字玄德
- 生卒：161-223
- 籍贯：涿郡涿县
- 史料定位：汉室宗亲后裔，蜀汉昭烈帝。
- 开局位置：涿郡在野 / 义兵
- 初始官职：无，黄巾后可为县尉/县令

**生平**

刘备早年织席贩履，因黄巾乱起募集乡勇。正史强调其有英雄气度、善得人心；演义强化仁德与桃园结义。184 年不宜直接作为朝堂大臣，但应作为民间义兵与宗室复兴线的重要冷备。

**游戏数据**

- Faction：在野名士 / 宗室 / 义军
- TitleTier：0
- BirthYear：161；BaseLongevity：62
- Favorability：50；Power：5；Corruption：2；StashedWealth：20
- 五维：Martial 60 / Leadership 72 / Politics 66 / Charisma 92 / Ambition 78
- Traits：`AiMinRuZi`, `QinMinWenHe`, `JingTianWeiDi`
- Personality：宽厚坚忍
- Style：仁义聚众
- 推荐用途：招安、地方安民、低成本义军；高魅力会逐步吸纳人才。
- 登场规则：黄巾爆发后在冀州/幽州义兵事件中登场。

### 4.25 关羽

**人物基础数据**

- ID：`guan_yu`
- 姓名：关羽，字云长
- 生卒：？-220
- 籍贯：河东解县
- 史料定位：刘备核心将领，后世武圣形象。
- 开局位置：在野逃亡 / 刘备义兵
- 初始官职：无

**生平**

关羽早年因事亡命，后随刘备起兵。正史称其“万人敌”，演义极大强化忠义、武勇、傲气。游戏中应作为高武力高忠义角色，但通常绑定刘备线，不宜被汉灵帝随意直接征辟为朝官，除非玩家走特殊“招募义士”路线。

**游戏数据**

- Faction：在野义军 / 刘备集团
- TitleTier：0
- BirthYear：160；BaseLongevity：60
- Favorability：45；Power：3；Corruption：0；StashedWealth：10
- 五维：Martial 96 / Leadership 84 / Politics 35 / Charisma 75 / Ambition 38
- Traits：`KongWuYouLi`, `ZhiJunYanZheng`, `GangZhiBuE`, `AiBingRuZi`
- Personality：傲岸重义
- Style：义不负主
- 推荐用途：高阶平叛将；若刘备受冷落，关羽不易单独效忠朝廷。
- 登场规则：刘备登场后作为随从触发。

### 4.26 张飞

**人物基础数据**

- ID：`zhang_fei`
- 姓名：张飞，字益德/翼德
- 生卒：？-221
- 籍贯：涿郡
- 史料定位：刘备核心将领，万人敌。
- 开局位置：涿郡义兵
- 初始官职：无

**生平**

张飞与关羽并称万人敌。正史中勇猛但性急，演义中豪放暴烈。游戏中适合作为高武力、较高统帅、低政治、纪律风险角色。若用于平叛，胜率高但可能降低地方民心。

**游戏数据**

- Faction：在野义军 / 刘备集团
- TitleTier：0
- BirthYear：160；BaseLongevity：61
- Favorability：42；Power：3；Corruption：8；StashedWealth：15
- 五维：Martial 94 / Leadership 78 / Politics 20 / Charisma 62 / Ambition 45
- Traits：`KongWuYouLi`, `YouXieLiQi`, `AiBingRuZi`
- 建议新增 Trait：`性烈鞭挞`（战斗威慑上升，治理/安抚下降）
- Personality：豪烈
- Style：猛攻震慑
- 推荐用途：军事平叛；不适合招安和地方治理。
- 登场规则：刘备登场后作为随从触发。

### 4.27 荀彧

**人物基础数据**

- ID：`xun_yu`
- 姓名：荀彧，字文若
- 生卒：163-212
- 籍贯：颍川颍阴
- 史料定位：王佐之才，曹操核心谋臣。
- 开局位置：颍川青年名士
- 初始官职：孝廉候选 / 尚未显达

**生平**

荀彧出身颍川荀氏，少年即有名望。184 年时年龄偏小，不宜作为成熟尚书令开局在朝。后为曹操规划根据地、人才与奉迎天子路线，同时坚持汉室名义。游戏中应作为后期顶级政略人才。

**游戏数据**

- Faction：清流派 / 颍川士族
- TitleTier：0
- BirthYear：163；BaseLongevity：50
- Favorability：48；Power：5；Corruption：0；StashedWealth：50
- 五维：Martial 10 / Leadership 35 / Politics 96 / Charisma 88 / Ambition 35
- Traits：`JingTianWeiDi`, `ShanChangMinZheng`, `QingZhengLianJie`, `LaoMouShenSuan`
- Personality：温雅深谋
- Style：王佐匡汉
- 推荐用途：政务顶级、赈灾/制度改革、朝会高质量发言；早期需征辟。
- 登场规则：185-188 颍川举荐或曹操势力上升时登场。

### 4.28 荀攸

**人物基础数据**

- ID：`xun_you`
- 姓名：荀攸，字公达
- 生卒：157-214
- 籍贯：颍川颍阴
- 史料定位：曹操谋主之一，长于军谋。
- 开局位置：洛阳/颍川士人
- 初始官职：黄门侍郎候补

**生平**

荀攸以奇策和军谋见长，董卓入洛后曾参与谋刺董卓而下狱。相比荀彧偏制度与大局，荀攸适合战术谋划和高风险密谋。游戏中可用于刺杀、离间、平叛谋略等系统。

**游戏数据**

- Faction：清流派 / 颍川士族
- TitleTier：1
- BirthYear：157；BaseLongevity：57
- Favorability：50；Power：12；Corruption：2；StashedWealth：70
- 五维：Martial 18 / Leadership 45 / Politics 88 / Charisma 70 / Ambition 32
- Traits：`LaoMouShenSuan`, `YouXieXinJi`, `QingZhengLianJie`
- Personality：沉密
- Style：奇策制敌
- 推荐用途：离间招安、谋刺事件、朝会军略；失败可能被捕。
- 登场规则：清流密谋或颍川举荐。

### 4.29 贾诩

**人物基础数据**

- ID：`jia_xu`
- 姓名：贾诩，字文和
- 生卒：147-223
- 籍贯：武威姑臧
- 史料定位：凉州谋士，善自保与奇谋。
- 开局位置：凉州/董卓集团边缘
- 初始官职：郡吏 / 军中谋士

**生平**

贾诩早年在凉州系统活动，后随董卓集团入局。董卓死后，劝李傕郭汜反攻长安，改变汉末局势。其一生以精准判断和自保著称。游戏中应是高智谋、低道德约束、极强风险管理的谋士。

**游戏数据**

- Faction：割据军阀 / 凉州集团
- TitleTier：1
- BirthYear：147；BaseLongevity：76
- Favorability：30；Power：10；Corruption：18；StashedWealth：100
- 五维：Martial 8 / Leadership 38 / Politics 92 / Charisma 55 / Ambition 40
- Traits：`LaoMouShenSuan`, `YouXieXinJi`
- 建议新增 Trait：`毒士自保`（危机中保全自身并提高敌对方破坏力）
- Personality：冷静诡谲
- Style：趋利避祸
- 推荐用途：高风险谋略、离间、敌方 AI 军师；不宜轻易给中央稳定加成。
- 登场规则：董卓/凉州线触发。

### 4.30 张辽

**人物基础数据**

- ID：`zhang_liao`
- 姓名：张辽，字文远
- 生卒：169-222
- 籍贯：雁门马邑
- 史料定位：曹魏名将，早年并州军系统。
- 开局位置：并州边军青年
- 初始官职：郡吏/小校

**生平**

张辽早年在并州丁原、何进、董卓、吕布等系统中辗转，后归曹操，成为五子良将之一。184 年时尚年轻，可作为边军青年冷备。其特点是高统帅、高纪律、比吕布稳定。

**游戏数据**

- Faction：边军 / 冷备名将
- TitleTier：0
- BirthYear：169；BaseLongevity：53
- Favorability：45；Power：2；Corruption：5；StashedWealth：20
- 五维：Martial 86 / Leadership 88 / Politics 42 / Charisma 68 / Ambition 45
- Traits：`ZhiJunYanZheng`, `DongDianBingFa`, `AiBingRuZi`
- Personality：沉勇
- Style：严整突击
- 推荐用途：后期平叛名将；早期需培养。
- 登场规则：并州军扩编、丁原线或吕布线后续触发。

### 4.31 韩馥

**人物基础数据**

- ID：`han_fu`
- 姓名：韩馥，字文节
- 生卒：？-约191
- 籍贯：颍川
- 史料定位：冀州牧，后让冀州于袁绍。
- 开局位置：洛阳/地方候任
- 初始官职：御史中丞或地方官候补

**生平**

韩馥后来任冀州牧，因政治判断不足，被袁绍夺取冀州。其适合作为“能力一般但资源州郡重要”的 NPC：任命他可暂时维持地方，但容易被门阀或军阀渗透。

**游戏数据**

- Faction：地方州牧 / 清流边缘
- TitleTier：2
- BirthYear：145；BaseLongevity：50
- Favorability：50；Power：20；Corruption：22；StashedWealth：250
- 五维：Martial 20 / Leadership 42 / Politics 55 / Charisma 45 / Ambition 35
- Traits：`CaiShuXueQian`, `QinMinWenHe`
- Personality：懦弱多疑
- Style：守土无断
- 推荐用途：冀州治理低风险但低效率；袁绍夺权剧情。
- 登场规则：冀州太守/州牧候选。

### 4.32 张角

**人物基础数据**

- ID：`zhang_jue`
- 姓名：张角
- 生卒：？-184
- 籍贯：冀州巨鹿
- 史料定位：太平道首领，黄巾起义领袖。
- 开局位置：敌对势力
- 初始官职：无；自号大贤良师、天公将军

**生平**

张角以太平道聚众，趁东汉末年灾荒、吏治腐败、民不聊生而发动黄巾起义。正史中其为黄巾首领，演义中带有符水、道术、天命叙事色彩。游戏中不应作为普通可任命臣子，而是危机系统核心敌对 NPC。

**游戏数据**

- Faction：反叛势力
- TitleTier：0
- BirthYear：140；BaseLongevity：44
- Favorability：0；Power：55；Corruption：5；StashedWealth：300
- 五维：Martial 25 / Leadership 78 / Politics 62 / Charisma 90 / Ambition 95
- Traits：`AiMinRuZi`, `QinMinWenHe`, `LaoMouShenSuan`
- 建议新增 Trait：`妖言惑众`（叛乱扩散与民众动员上升）
- Personality：狂热救世
- Style：宗教动员
- 推荐用途：黄巾主线 Boss、招安几乎不可行；其死亡会削弱但不消灭黄巾。
- 登场规则：黄巾之乱自动敌对登场。

## 5. 第二批扩展名单（简版数据，后续展开）

以下人物建议作为第二批补全，先进入 preset 池，暂不全部写长传。

- `zhang_bao` 张宝：张角弟，地公将军；黄巾副首领；Leadership 65 / Charisma 75 / Ambition 88。
- `zhang_liang` 张梁：张角弟，人公将军；黄巾副首领；Martial 55 / Leadership 68 / Ambition 86。
- `zhang_yan` 张燕：黑山军首领；山地流寇/可招安；Martial 70 / Leadership 78 / Charisma 70。
- `ma_teng` 马腾：凉州军阀，后伏波将军之后；Martial 72 / Leadership 72 / Ambition 62。
- `han_sui` 韩遂：凉州叛乱首领；Politics 72 / Leadership 70 / Ambition 86。
- `gongsun_du` 公孙度：辽东太守，后割据辽东；Politics 60 / Leadership 65 / Ambition 78。
- `qiao_mao` 乔瑁：东郡太守，关东诸侯之一；Politics 52 / Charisma 55 / Ambition 48。
- `zhang_miao` 张邈：陈留太守，名士，与曹操关系复杂；Charisma 72 / Politics 62 / Ambition 55。
- `bao_xin` 鲍信：济北相，早期支持曹操；Leadership 62 / Politics 58 / Ambition 35。
- `yuan_yi` 袁遗：袁氏支系，山阳太守；门阀辅助人物。
- `chen_qian` 陈谦/陈宫可后续取舍：若加入陈宫，应作为兖州谋士，Politics 82 / Ambition 45。
- `guo_jia` 郭嘉：年龄偏小，后期鬼才谋士；Politics 92 / Charisma 72 / Health 较低。
- `cheng_yu` 程昱：兖州谋士，强硬；Politics 84 / Leadership 55 / Style 强硬。
- `tian_feng` 田丰：袁绍谋士，刚直；Politics 86 / Trait `GangZhiBuE`。
- `ju_shou` 沮授：袁绍谋士，大局强；Politics 88 / Leadership 62。
- `gao_shun` 高顺：陷阵营将领；Martial 82 / Leadership 86 / Trait `ZhiJunYanZheng`。
- `li_jue` 李傕：董卓部将；Martial 68 / Leadership 55 / Ambition 80 / Corruption 70。
- `guo_si` 郭汜：已有 fallback，可补传记；粗暴军阀。

## 6. 建议新增 Trait

现有 Trait 足够支撑第一批，但为了让人物差异更鲜明，建议新增以下特质：

- `反复无常`：低好感、受贿、敌方拉拢时叛变概率上升；代表吕布、部分边将。
- `妖言惑众`：叛乱扩散、民众动员、黄巾响应上升；代表张角。
- `毒士自保`：谋略成功率高，失败时自身退场概率低，但可能提高民心/皇权副作用；代表贾诩。
- `名士清望`：朝会发言影响清流与士人舆论；被冤杀会降低皇权/民心；代表孔融、蔡邕。
- `州牧自专`：外任地方时治理效率上升，但中央控制下降，叛乱类型从民变转为割据风险；代表刘焉、刘表。
- `边军宿将`：边州平叛与异族战争加成，入京带兵时宫变风险上升；代表董卓、丁原、公孙瓒。

## 7. 落地顺序建议

### P0：先补足“可玩候选池”

- 新增 `NpcPresetDatabase` 或扩展当前 `NpcLifecycleManager` hardcoded fallback。
- 第一批落地 20-24 个：袁绍、袁术、王允、卢植、皇甫嵩、朱儁、赵忠、段珪、毕岚、何苗、杨彪、马日磾、蔡邕、董卓、丁原、刘焉、刘虞、刘表、陶谦、孔融、公孙瓒、孙坚、刘备、张角。
- 开局朝堂不要全部激活：只激活洛阳 10-14 人；地方/敌对放入 preset 池。

### P1：补“登场规则”

- `EntryYear`、`EntryCondition`、`InitialLocation` 可作为未来扩展字段。
- 当前 `NpcState` 没有这些字段，短期可用单独 `NpcPresetMetadata` 或 JSON 外层包装。
- 黄巾触发时部署张角/张宝/张梁为敌对势力，不进入普通 Npcs 任命列表。

### P2：补“朝会 AI 个性”

为每人准备 2-3 条朝会立场模板：

- 阉党：支持卖官、反对削内廷、攻击清流。
- 外戚：支持扩军、排斥宦官、索要军费。
- 清流：反腐、赈灾、抑制外兵入京。
- 地方州牧：请求放权、要求粮饷、隐藏自专倾向。
- 边军：要求军费、战马、兵权；低皇权时抗命概率上升。

## 8. 关键平衡原则

- **不能把未来英雄都做成开局神臣**：刘备、荀彧、张辽等应低 Power、低 TitleTier，需要玩家识人培养。
- **高能力必须绑定风险**：董卓/吕布战力强但忠诚风险高；袁氏政治强但门阀反噬强；宦官能给钱但腐败毁民心。
- **清流不是免费答案**：清流廉洁但刚直，容易与皇帝享乐、卖官、西园私库冲突。
- **地方治理要产生军阀化压力**：太守/州牧能力越强，长期外任越可能形成独立势力。
- **演义形象用于增强辨识度，正史用于确定基本定位**：例如王允可有连环计叙事，但游戏数值不应只按演义夸张。

## 9. 下一步实现计划

1. 把本文第一批人物转成 `NpcPreset` 数据文件或 C# fallback。
2. 给 `NpcState` 或外层 metadata 增加：`InitialLocation`、`EntryCondition`、`HistoricalRole`、`IsHostile`。
3. 前端大臣列表增加筛选：在朝 / 地方 / 在野 / 敌对，不让张角这类敌对人物出现在任命太守列表。
4. 测试覆盖：
   - preset 数量不少于 30；ID 唯一。
   - 所有 Traits 必须存在于 `TraitNames` 或标记为建议新增。
   - 初始开局激活人数可控，不能一次性把冷备全部塞进 `GameState.Npcs`。
   - 平叛/招安候选不包含敌对首领和未登场冷备。
