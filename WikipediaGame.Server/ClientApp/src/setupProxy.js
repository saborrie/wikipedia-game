const { createProxyMiddleware } = require("http-proxy-middleware");

module.exports = function (app) {
  app.use(
    createProxyMiddleware("/hubs/**", {
      target: "http://localhost:5240",
      changeOrigin: true,
    })
  );

  app.use(
    createProxyMiddleware("/hubs/**", {
      target: "ws://localhost:5240",
      ws: true,
      changeOrigin: true,
    })
  );
};
