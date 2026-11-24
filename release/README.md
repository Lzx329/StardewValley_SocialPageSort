# Social Page Sort

给星露谷物语 1.6 的社交页加一个排序按钮：按好感、名字（拼音）或生日排序。
A small SMAPI mod that lets you sort the social page by friendship, name (pinyin) or birthday.

- 游戏版本：Stardew Valley 1.6
- SMAPI：4.x（开发与验证环境为 4.5.2）
- 依赖：无（Generic Mod Config Menu 可选）
- UniqueID：`lzxlx.SocialPageSort`

## 功能

- 社交页标签条最左侧新增一个“排序”按钮，点开后可选择三种排序方式。
- **好感**：按心等级（再按精确好感点数）排序，默认降序——好感最高的在最上面。
- **名字（拼音）**：以汉字开头的名字按普通话拼音排序；英文、数字等非汉字名按字母序，并整体排在汉字名之后。默认升序。
- **生日**：按距离下一个生日的天数排序，今天生日为 0 天。默认升序。
- 玩家角色固定在最前面，未见面角色固定排在已见面角色之后。
- 当前排序方式和方向会写进 `config.json`，下次启动沿用。

## 安装

1. 安装 SMAPI 4.x。
2. 解压发布包，把里面的 `SocialPageSort` 文件夹整个放进游戏的 `Mods` 目录。
3. 启动游戏。日志中出现 `Social Page Sort 1.0.0` 即加载成功。

目录结构应为：

```
Mods/
  SocialPageSort/
    manifest.json
    SocialPageSort.dll
    i18n/
      default.json
      zh.json
```

可选：安装 [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)，即可在“选项 → Mod 选项”里切换排序方式与方向；不装也能用页面上的按钮。

## 卸载

删除 `Mods\SocialPageSort` 文件夹即可。本 mod 不修改存档，也不写入存档数据；可顺手删除 `config.json`（如果存在）。

## 使用

1. 按 `E` 打开菜单，切到“社交”标签。
2. 点击标签条最左侧的“排序”按钮（宽度不足时显示为三横线图标），展开面板。
3. 面板里有三个选项：好感、名字（拼音）、生日。
   - **点名称**：按该方式的默认方向排序（好感降序、名字升序、生日升序）。
   - **点右侧箭头**：在当前方向上翻转（升序 ↔ 降序）。
4. 当前使用的选项左侧有深色竖条、背景为浅黄色。
5. 点击面板外的任意位置关闭面板，不会改变当前排序。

修改后立即生效并保存，无需重启游戏。

## 配置（config.json）

首次运行会在 mod 文件夹生成 `config.json`：

```json
{
  "SortMode": "Friendship",
  "FriendshipDescending": true,
  "NameDescending": false,
  "BirthdayDescending": false
}
```

| 字段 | 取值 | 默认 | 含义 |
|---|---|---|---|
| `SortMode` | `Friendship` / `Name` / `Birthday` | `Friendship` | 当前排序方式 |
| `FriendshipDescending` | `true` / `false` | `true` | 好感排序方向（`true` = 高到低） |
| `NameDescending` | `true` / `false` | `false` | 名字排序方向（`false` = 升序） |
| `BirthdayDescending` | `true` / `false` | `false` | 生日排序方向（`false` = 近到远） |

`config.json` 缺失、损坏或字段非法时，会回退到“好感降序”并写一条 warning 日志，不会阻止游戏启动。

## 已知行为

- **英文名在名字排序中沉底**：名字排序分两段，汉字名在前、非汉字名（英文、数字等）在后。升降序只改变段内顺序，不会让英文名跑到汉字名前面。比较时忽略大小写、全半角与变音符号；显示名相同时用内部名兜底，保证多次打开顺序一致。
- **好感排序与原版顺序不完全相同**：原版顺序是“好感点数降序 → 见面先后 → 显示名”，未见面角色和孩子在末尾；本 mod 是“心等级 → 精确点数 → 名字规则”，未见面角色固定排在已见面角色之后。两者主键相同，但同分时的先后可能不同。
- 菜单中送礼导致好感变化时，列表不会当场重排；关闭菜单或切换标签后重新进入社交页即可看到新顺序。
- 无好感数据的条目排在有数据条目之后；无生日数据的条目同理，且保持原有相对顺序。
- 排序按钮目前只支持鼠标点击，不支持手柄/键盘导航。
- 按钮固定位于标签条最左侧空白格，这是为了避开 UI Info Suite 2 等 mod 在右上角自绘的按钮，并非可配置项。

## 兼容性

- 与 UI Info Suite 2、LookupAnything、Ridgeside Village 同开无报错。
- 只读取社交页的数据并重排顺序，不修改 NPC 数据、不修改存档，也不改动其他 mod 的资源。
- 如果某个环节出现异常，会保留原版顺序并继续运行，不会让游戏崩溃。

## 声明

- **AI 参与声明**：本 mod 的代码由 AI 辅助生成，并经实机测试与人工审核。使用时请自行评估风险。
- **独立实现声明**：本项目为独立实现，不含 aedanthorn 的 SocialPageOrderButton 的代码或资源，与其没有继承、分支或衍生关系。功能和交互是重新设计的。

## 许可证

本仓库暂未附带 `LICENSE` 文件——许可证由作者自行选择后补上。在选定之前，默认保留所有权利（All rights reserved），他人只能查看，不能复制、修改或再分发。

如果希望允许他人自由使用与分发，常见选择：

- **MIT**：最宽松，只需保留版权声明与许可声明，适合大多数 SMAPI mod。
- **GPL-3.0**：要求衍生作品也开源，适合希望保持社区共享的项目。

选定后在仓库根目录添加 `LICENSE` 文件，并在本 README 末尾补一句许可证名称即可。

## 关于 Nexus

当前版本只面向 GitHub 发布。如果以后要上 Nexus Mods，需要额外准备：

1. 在 Nexus 创建 mod 页面，拿到 mod ID，把它填进 `manifest.json` 的 `UpdateKeys`（如 `"Nexus:123456"`），这样 SMAPI 才能检查更新。
2. 准备页面素材：封面图、至少几张游戏内截图、简短的英文描述与安装说明（Nexus 面向英文用户，建议提供英文版说明）。
3. 上传发布包 zip，并填写版本号、游戏版本、SMAPI 最低版本等元数据。
4. 设置 mod 权限（是否允许转载、是否允许他人修改后发布），并确认与 GitHub 仓库的许可证一致。
5. 可选：在 README 与 Nexus 页面互相链接，声明 GitHub 为主要开发仓库。

## 发布到 GitHub 的步骤

1. 在 GitHub 新建仓库，把 `release/README.md` 放到仓库根目录作为 README。
2. 建议把源码目录 `src/` 一并提交（可公开发布，也可只发布成品）。若只发布成品，至少保留 `manifest.json`、`i18n/` 与 `SocialPageSort.dll` 的说明。
3. 打 tag `v1.0.0`，新建 Release，标题写 `Social Page Sort 1.0.0`，把 `SocialPageSort-1.0.0.zip` 作为附件上传。
4. 在 Release 说明里写清安装方法（解压后放进 `Mods`），并注明需要 SMAPI 4.x。
