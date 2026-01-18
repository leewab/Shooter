const http = require('http');
const fs = require('fs');
const path = require('path');

const PORT = process.env.PORT || 3000;
const PUBLIC_DIR = path.join(__dirname, 'public', 'resources');

// MIME类型映射
const mimeTypes = {
  '.html': 'text/html',
  '.js': 'text/javascript',
  '.css': 'text/css',
  '.json': 'application/json',
  '.png': 'image/png',
  '.jpg': 'image/jpg',
  '.gif': 'image/gif',
  '.mp3': 'audio/mpeg',
  '.mp4': 'video/mp4',
  '.unity3d': 'application/octet-stream',
  '.u3d': 'application/octet-stream',
  '.bundle': 'application/octet-stream',
  '.ab': 'application/octet-stream'
};

// 创建HTTP服务器
const server = http.createServer((req, res) => {
  console.log(`[${new Date().toISOString()}] ${req.method} ${req.url}`);
  
  // 设置CORS头
  res.setHeader('Access-Control-Allow-Origin', '*');
  res.setHeader('Access-Control-Allow-Methods', 'GET, OPTIONS');
  res.setHeader('Access-Control-Allow-Headers', 'Content-Type');
  
  // 处理OPTIONS请求
  if (req.method === 'OPTIONS') {
    res.writeHead(200);
    res.end();
    return;
  }
  
  // 健康检查路由
  if (req.url === '/health') {
    res.writeHead(200, { 'Content-Type': 'application/json' });
    res.end(JSON.stringify({ status: 'ok', message: '微信小游戏资源服务器运行正常' }));
    return;
  }
  
  // 处理资源请求
  if (req.url.startsWith('/resources/')) {
    // 提取资源路径
    const resourcePath = req.url.substring('/resources/'.length);
    const filePath = path.join(PUBLIC_DIR, resourcePath);
    
    // 防止路径遍历攻击
    if (!filePath.startsWith(PUBLIC_DIR)) {
      res.writeHead(403, { 'Content-Type': 'text/plain' });
      res.end('Forbidden');
      return;
    }
    
    // 检查文件是否存在
    fs.access(filePath, fs.constants.F_OK, (err) => {
      if (err) {
        res.writeHead(404, { 'Content-Type': 'text/plain' });
        res.end('File Not Found');
        return;
      }
      
      // 确定文件MIME类型
      const extname = path.extname(filePath).toLowerCase();
      const contentType = mimeTypes[extname] || 'application/octet-stream';
      
      // 读取并返回文件
      fs.readFile(filePath, (err, content) => {
        if (err) {
          res.writeHead(500, { 'Content-Type': 'text/plain' });
          res.end('Server Error');
          return;
        }
        
        res.writeHead(200, { 'Content-Type': contentType });
        res.end(content);
      });
    });
    return;
  }
  
  // 处理其他请求
  res.writeHead(404, { 'Content-Type': 'text/plain' });
  res.end('Not Found');
});

// 启动服务器
server.listen(PORT, () => {
  console.log(`微信小游戏资源服务器已启动，访问地址: http://localhost:${PORT}`);
  console.log(`资源访问路径: http://localhost:${PORT}/resources/`);
  console.log(`健康检查: http://localhost:${PORT}/health`);
  console.log(`
使用说明:`);
  console.log(`1. 将Unity导出的资源放入: ${PUBLIC_DIR}`);
  console.log(`2. 通过以下URL访问资源: http://localhost:${PORT}/resources/[资源文件路径]`);
  console.log(`3. 例如: http://localhost:${PORT}/resources/assetbundle/character.unity3d`);
});
