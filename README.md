# v2-ep — 2dust/v2rayN + encrypted-proxy

基于 [2dust/v2rayN](https://github.com/2dust/v2rayN) 的 fork，新增 **encrypted-proxy** 协议支持。在保留 v2rayN 全部原版功能（Xray / sing-box / mihomo 多核心、TUN、路由、订阅、测速）的基础上，可原生添加 [encrypted-proxy](https://github.com/firexbox/encrypted-proxy)（AES-256-GCM 加密 + 流量混淆）节点。

## 功能

| 组件 | 说明 |
|------|------|
| **加密代理节点** | 服务器菜单 → 添加[加密代理]服务器，支持密码 + 流量混淆(obfs) + 时序抖动(jitter) 开关 |
| **ep:// 分享链接** | 支持导入/导出，格式：`ep://密码@主机:端口?obfs=1&jitter=1#备注` |
| **独立核心** | 内置 encrypted-proxy 客户端核心（bin/encryptedproxy/），以 `-config config.json` 方式启动，自动监听 v2rayN 配置的本地 SOCKS/HTTP 端口 |
| **原版功能** | 全部保留 — Xray/sing-box/mihomo 核心、TUN 模式、路由分流、订阅更新、测速、剪贴板/二维码导入等 |

## 修改清单

| 文件 | 说明 |
|------|------|
| `ServiceLib/Enums/EConfigType.cs` | 新增 `EncryptedProxy = 14` |
| `ServiceLib/Enums/ECoreType.cs` | 新增 `encryptedproxy = 31` |
| `ServiceLib/Global.cs` | `ep://` 协议头、`encryptedproxy` 协议名、核心发布源 |
| `ServiceLib/Handler/Fmt/EpFmt.cs` | 新文件 — ep:// 链接解析与导出 |
| `ServiceLib/Services/CoreConfig/Ep/CoreConfigEpService.cs` | 新文件 — 生成 EP 客户端 config.json |
| `ServiceLib/Manager/CoreInfoManager.cs` | 注册 encrypted-proxy 核心（`-config {0}` 启动） |
| `ServiceLib/Handler/ConfigHandler.cs` | `AddEncryptedProxyServer` 保存处理 |
| `ServiceLib/Handler/Builder/NodeValidator.cs` | EP 节点校验（必填密码） |
| `ServiceLib/ViewModels/*` | 添加命令、EP 字段（obfs/jitter）读写 |
| `v2rayN/Views/*` + `v2rayN.Desktop/Views/*` | WPF 与 Avalonia 双 UI：菜单项 + 编辑窗口字段组 |

## 下载 / 构建

| 平台 | 架构 | 下载 |
|------|------|------|
| Windows | x64 | [v2rayN-windows-64.zip](https://github.com/firexbox/v2-ep/releases) |

Windows 包已内置 encrypted-proxy 核心与官方 xray/sing-box/mihomo 核心，解压即用。

自行构建（Windows 包，Linux/macOS 可交叉编译）：

```bash
# 需要 .NET 10 SDK
dotnet publish ./v2rayN/v2rayN.csproj -c Release -r win-x64 -p:SelfContained=true -p:EnableWindowsTargeting=true
# 将 encrypted-proxy-windows-amd64.exe 放入输出目录 bin/encryptedproxy/encrypted-proxy.exe
```

## 快速开始

1. 解压运行 `v2rayN.exe`，点击菜单 **服务器 → 添加[加密代理]服务器**
2. 填写：地址、端口、密码（勾选"流量混淆"以与服务端 `-obfs` 参数匹配）
3. 或直接粘贴分享链接：`ep://your-password@your-server.com:8388?obfs=1#my-ep`
4. 选中节点 → 回车启动，系统代理自动生效（默认 127.0.0.1:10808）

服务端为 [encrypted-proxy](https://github.com/firexbox/encrypted-proxy) 项目：

```bash
encrypted-proxy -mode server -listen 0.0.0.0:8388 -password 'your-password' -obfs
```

> **注意**：客户端与服务端的 `-obfs` / `-jitter` 参数必须一致，否则连接建立后数据无法正常转发。

## 常见问题

| 问题 | 解决 |
|------|------|
| 启动提示未找到 encrypted-proxy 核心 | 检查 `bin/encryptedproxy/encrypted-proxy.exe` 是否存在；也可在"更新核心"中选择 encryptedproxy 手动下载 |
| 节点连接成功但网页打不开 | 确认服务端 `-obfs`/`-jitter` 与节点开关一致；服务端密码与节点密码一致 |
| 测速/统计对 EP 节点无效 | EP 使用独立核心，不参与 Xray/sing-box 测速（与原版自定义核心行为一致） |
| TUN 模式与 EP 节点 | TUN 由 xray/sing-box 前置服务实现，EP 节点建议使用系统代理模式 |

## 许可证

[GPL-3.0](LICENSE) — 继承自 v2rayN 项目。

---

# v2-ep — 2dust/v2rayN + encrypted-proxy (English)

A fork of [2dust/v2rayN](https://github.com/2dust/v2rayN) with native support for the **encrypted-proxy** protocol (AES-256-GCM + obfuscation). All upstream features are preserved: Xray / sing-box / mihomo cores, TUN mode, routing, subscriptions, and speed tests.

## Features

| Component | Description |
|-----------|-------------|
| **EP nodes** | Servers menu → Add [Encrypted Proxy]; password + obfs + jitter toggles |
| **ep:// links** | `ep://password@host:port?obfs=1&jitter=1#remarks` — import/export supported |
| **Dedicated core** | Bundled encrypted-proxy client (bin/encryptedproxy/) launched via `-config config.json`, listening on v2rayN's configured local SOCKS/HTTP port |
| **Upstream features** | All preserved — Xray/sing-box/mihomo cores, TUN, routing, subscriptions, QR/clipboard import |

## Changes

| File | Description |
|------|-------------|
| `ServiceLib/Enums/EConfigType.cs` | Added `EncryptedProxy = 14` |
| `ServiceLib/Enums/ECoreType.cs` | Added `encryptedproxy = 31` |
| `ServiceLib/Global.cs` | `ep://` scheme, protocol name, core source repo |
| `ServiceLib/Handler/Fmt/EpFmt.cs` | New — ep:// link parse/export |
| `ServiceLib/Services/CoreConfig/Ep/CoreConfigEpService.cs` | New — EP client config.json generator |
| `ServiceLib/Manager/CoreInfoManager.cs` | Registers the encrypted-proxy core (`-config {0}` launch) |
| `ServiceLib/Handler/ConfigHandler.cs` | `AddEncryptedProxyServer` persistence |
| `ServiceLib/Handler/Builder/NodeValidator.cs` | EP validation (password required) |
| `ServiceLib/ViewModels/*` | Add command + EP fields (obfs/jitter) |
| `v2rayN/Views/*` + `v2rayN.Desktop/Views/*` | WPF + Avalonia UI: menu item + editor field group |

## Download / Build

| Platform | Arch | Download |
|----------|------|----------|
| Windows | x64 | [v2rayN-windows-64.zip](https://github.com/firexbox/v2-ep/releases) |

The Windows package bundles the encrypted-proxy core plus the official xray / sing-box / mihomo cores — unzip and run.

Build from source (cross-compile on Linux/macOS with .NET 10 SDK):

```bash
dotnet publish ./v2rayN/v2rayN.csproj -c Release -r win-x64 -p:SelfContained=true -p:EnableWindowsTargeting=true
# copy encrypted-proxy-windows-amd64.exe to output/bin/encryptedproxy/encrypted-proxy.exe
```

## Quick Start

1. Run `v2rayN.exe` → menu **Servers → Add [Encrypted Proxy]**
2. Fill in address, port, password (enable "obfs" to match the server's `-obfs` flag)
3. Or paste a share link: `ep://your-password@your-server.com:8388?obfs=1#my-ep`
4. Select the node and press Enter to start; system proxy is applied automatically (default 127.0.0.1:10808)

Server side: [encrypted-proxy](https://github.com/firexbox/encrypted-proxy)

```bash
encrypted-proxy -mode server -listen 0.0.0.0:8388 -password 'your-password' -obfs
```

> **Note**: `-obfs` / `-jitter` flags must match between client and server, otherwise the tunnel connects but data does not flow.

## FAQ

| Problem | Solution |
|---------|----------|
| "Core not found" for encryptedproxy | Ensure `bin/encryptedproxy/encrypted-proxy.exe` exists, or download it via the core-update dialog |
| Node connects but pages time out | Check obfs/jitter toggles and password match the server settings |
| Speed test unavailable for EP nodes | EP uses its own core and is excluded from Xray/sing-box speed tests (same as upstream custom cores) |
| TUN mode with EP nodes | TUN relies on the xray/sing-box pre-service; use system-proxy mode for EP nodes |

## License

[GPL-3.0](LICENSE) — inherited from the v2rayN project.
