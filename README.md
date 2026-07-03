# Monster Master

Unity 游戏项目。

- **引擎版本**：Unity 2022.3 LTS
- **默认分辨率**：1920 × 1080

## 目录结构

```
monster-master/
├── Assets/                    # 所有游戏资源（Unity 核心目录）
│   ├── Art/                   # 美术资源
│   │   ├── Audio/
│   │   │   ├── Music/         # 背景音乐
│   │   │   └── SFX/           # 音效
│   │   ├── Fonts/             # 字体
│   │   ├── Materials/         # 材质
│   │   ├── Models/            # 3D 模型
│   │   └── Textures/          # 贴图
│   │       ├── Characters/    # 角色贴图
│   │       ├── Environments/  # 场景 / 环境贴图
│   │       └── UI/            # UI 贴图与图标
│   ├── Data/                  # 配置数据（ScriptableObject、JSON 等）
│   ├── Prefabs/               # 预制体
│   │   ├── Characters/        # 角色预制体
│   │   ├── UI/                # UI 预制体
│   │   └── World/             # 世界 / 地图预制体
│   ├── Resources/             # 运行时动态加载资源（Resources.Load）
│   ├── Scenes/                # 场景文件（.unity）
│   │   ├── Levels/            # 关卡场景
│   │   ├── UI/                # UI 场景
│   │   └── World/             # 世界 / 地图场景
│   ├── Scripts/               # C# 脚本
│   │   ├── Core/              # 全局管理器、工具类、基础框架
│   │   ├── Characters/        # 角色相关逻辑
│   │   ├── Systems/           # 游戏系统（战斗、背包、任务等）
│   │   └── UI/                # UI 逻辑与交互
│   └── Settings/              # 项目设置资源（Input Actions、渲染管线等）
├── Packages/                  # Unity 包管理配置
├── ProjectSettings/           # Unity 项目设置
├── docs/                      # 设计文档、策划说明
├── .gitignore
└── README.md
```

## 各目录用途

| 目录 | 用途 |
|------|------|
| `Assets/Art/` | 存放美术原始资源，包括音频、字体、材质、模型和贴图 |
| `Assets/Art/Audio/Music/` | 背景音乐（BGM） |
| `Assets/Art/Audio/SFX/` | 音效（点击、攻击、环境音等） |
| `Assets/Art/Fonts/` | 游戏内使用的字体文件 |
| `Assets/Art/Materials/` | 材质球（Material） |
| `Assets/Art/Models/` | 3D 模型（角色、道具、场景物件等） |
| `Assets/Art/Textures/` | 贴图资源，按角色、环境、UI 分类存放 |
| `Assets/Data/` | 游戏配置数据，如怪物属性、关卡参数、掉落表等 |
| `Assets/Prefabs/` | 可复用的预制体，便于在多个场景中实例化 |
| `Assets/Resources/` | 需要通过代码动态加载的资源（谨慎使用，适合少量全局资源） |
| `Assets/Scenes/` | Unity 场景文件，按关卡、UI、世界分类 |
| `Assets/Scripts/Core/` | 核心框架代码，如 GameManager、事件系统、存档等 |
| `Assets/Scripts/Characters/` | 角色控制器、动画、状态机等 |
| `Assets/Scripts/Systems/` | 各游戏子系统的业务逻辑 |
| `Assets/Scripts/UI/` | 界面脚本，如菜单、HUD、弹窗等 |
| `Assets/Settings/` | ScriptableObject 配置、输入映射、渲染管线等资源 |
| `Packages/` | Unity Package Manager 依赖声明 |
| `ProjectSettings/` | 项目级配置（分辨率、构建目标、标签层等） |
| `docs/` | 策划文档、设计稿说明、开发笔记等 |

## 开发说明

1. 使用 **Unity Hub** 打开本项目根目录。
2. 首次打开会生成 `Library/` 目录及 `.meta` 文件，`.gitignore` 已忽略 `Library/` 和 `Temp/`。
3. 新建场景放入 `Assets/Scenes/` 对应子目录，脚本放入 `Assets/Scripts/` 对应子目录，保持资源与代码分类一致。
