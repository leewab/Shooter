# 微信小游戏资源服务器

一个极简的HTTP静态文件服务器，用于提供微信小游戏资源文件的访问。

## 功能特点

- ✅ 简单易用：只需几行命令即可启动
- ✅ HTTP访问：支持通过HTTP访问资源文件
- ✅ 跨域支持：已配置CORS，可被微信小游戏正常访问
- ✅ 资源组织：统一的资源存放目录

## 快速开始

### 1. 安装依赖

```bash
npm install
```

### 2. 准备资源

将Unity导出的微信小游戏资源文件放入以下目录：

```
public/resources/
```

### 3. 启动服务器

```bash
npm start
```

服务器将在 `http://localhost:3000` 启动。

### 4. 访问资源

通过以下URL访问资源文件：

```
http://localhost:3000/resources/[资源文件路径]
```

例如：
```
http://localhost:3000/resources/assetbundle/character.unity3d
http://localhost:3000/resources/audio/music.mp3
```

## 目录结构

```
CloudServer/
├── public/
│   └── resources/      # 资源文件存放目录
├── app.js              # 服务器入口文件
├── package.json        # 项目依赖配置
└── README.md           # 本说明文件
```

## 配置说明

### 修改端口

默认端口为3000，如需修改，可在 `app.js` 文件中修改 `PORT` 变量：

```javascript
const PORT = process.env.PORT || 8080;  // 修改为自定义端口
```

或者通过环境变量设置：

```bash
PORT=8080 npm start
```

### 修改资源目录

默认资源目录为 `public/resources/`，如需修改，可在 `app.js` 文件中修改静态资源配置：

```javascript
app.use('/resources', express.static(path.join(__dirname, 'custom', 'resources')));
```

## 微信小游戏使用示例

在微信小游戏代码中，可以通过以下方式加载资源：

```javascript
// 加载图片资源
const imageUrl = 'http://你的服务器IP:3000/resources/images/icon.png';
wx.loadImage(imageUrl, {
  success: (res) => {
    console.log('图片加载成功', res);
  }
});

// 加载AssetBundle
const bundleUrl = 'http://你的服务器IP:3000/resources/assetbundle/ui.u3d';
UnityLoader.loadBundle(bundleUrl, (bundle) => {
  console.log('AssetBundle加载成功');
});
```

## 生产环境部署

### 1. 使用PM2守护进程

```bash
# 安装PM2
npm install -g pm2

# 启动服务器
pm2 start app.js --name "game-res-server"

# 查看状态
pm2 status
```

### 2. 配置域名和HTTPS

建议在生产环境中配置域名和HTTPS，以提高安全性和兼容性：

1. 申请域名
2. 配置DNS解析到服务器IP
3. 安装SSL证书（可使用Let's Encrypt免费证书）
4. 使用Nginx或Caddy作为反向代理

## 注意事项

1. 确保服务器有足够的存储空间存放资源文件
2. 定期备份重要资源文件
3. 生产环境建议配置防火墙，限制访问IP
4. 微信小游戏要求请求必须使用HTTPS（开发环境除外）

## 版本历史

- v1.0.0：初始版本，提供基本的静态资源访问功能
