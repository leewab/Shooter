const express = require('express');
const cors = require('cors');
const path = require('path');

const app = express();
const PORT = process.env.PORT || 3000;

// 启用CORS支持
app.use(cors());

// 设置静态资源目录
app.use('/resources', express.static(path.join(__dirname, 'public', 'resources')));

// 健康检查路由
app.get('/health', (req, res) => {
  res.json({ status: 'ok', message: '微信小游戏资源服务器运行正常' });
});

// 启动服务器
app.listen(PORT, () => {
  console.log(`微信小游戏资源服务器已启动，访问地址: http://localhost:${PORT}`);
  console.log(`资源访问路径: http://localhost:${PORT}/resources/`);
});
