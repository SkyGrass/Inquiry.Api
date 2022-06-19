namespace Backend.Api.Auth
{
    /// <summary>
    /// JWT授权的配置项
    /// </summary>
    public class AppAuthenticationSettings
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        public string AppId { get; set; }
        /// <summary>
        /// 应用密钥(真实项目中可能区分应用,不同的应用对应惟一的密钥)
        /// </summary>
        public string Secret { get; set; }
        /// <summary>
        /// 接口请求地址
        /// </summary>
        public string NetUrl { get; set; }
        /// <summary>
        /// 授权重定向地址
        /// </summary>
        public string RedirectUrl { get; set; }
        /// <summary>
        /// 当前系统ID
        /// </summary>
        public string Id { get; set; }
    }
}