var path = require("path");
var webpack = require('webpack');
var HtmlWebpackPlugin = require('html-webpack-plugin');
var CopyWebpackPlugin = require('copy-webpack-plugin');
var MiniCssExtractPlugin = require('mini-css-extract-plugin');

const getBaseUrl = env => (env.baseUrl) ? `/${env.baseUrl}/` : "/";
const isProduction = env => (env.production) ? true : false;
// The HtmlWebpackPlugin allows us to use a template for the index.html page
// and automatically injects <script> or <link> tags for generated bundles.
var commonPlugins = env => [
    new HtmlWebpackPlugin({
        filename: 'index.html',
        template: resolve(CONFIG.indexHtmlTemplate),
        'base': { 'href': getBaseUrl(env) }
    }),
    new webpack.DefinePlugin({
        baseUrl: JSON.stringify(env.baseUrl),
    })
];

var CONFIG = {
    // The tags to include the generated JS and CSS will be automatically injected in the HTML template
    // See https://github.com/jantimon/html-webpack-plugin
    indexHtmlTemplate: './src/MyPlanner.Client.View/wwwroot/index.html',
    fsharpEntry: './src/MyPlanner.Client.View/fable-output/App.js',
    outputDir: './deploy/wwwroot',
    assetsDir: './src/MyPlanner.Client.View/wwwroot',
    devServerPort: 8080,
    // When using webpack-dev-server, you may need to redirect some calls
    // to a external API server. See https://webpack.js.org/configuration/dev-server/#devserver-proxy
    devServerProxy: {
        // redirect requests that start with /api/* to the server on port 8085
        '/api/*': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
            changeOrigin: true
        },
        // redirect websocket requests that start with /socket/* to the server on the port 8085
        '/socket/*': {
            target: 'http://localhost:' + (process.env.SERVER_PROXY_PORT || "8085"),
            ws: true
        }
    }
}

module.exports = env => ({
    entry: (isProduction(env)) ? {
        app: [resolve(CONFIG.fsharpEntry)]
    } : {
            app: [resolve(CONFIG.fsharpEntry)],
        },
    // Add a hash to the output file name in production
    // to prevent browser caching if code changes
    output: {
        path: resolve(CONFIG.outputDir),
        filename: isProduction ? '[name].[chunkhash].js' : '[name].js'
    },
    mode: (isProduction(env))  ? 'production' : 'development',
    devtool: (isProduction(env))  ? 'nosources-source-map' : 'eval-source-map',
    optimization: {
        runtimeChunk: 'single',
        splitChunks: {
            chunks: 'all'
        },
        // NOTE: Toggle javascript minimized output
        //       Use if you want to debug the generated javascript code
        minimize: !(isProduction(env))
    },
    // Besides the HtmlPlugin, we use the following plugins:
    // PRODUCTION
    //      - MiniCssExtractPlugin: Extracts CSS from bundle to a different file
    //          To minify CSS, see https://github.com/webpack-contrib/mini-css-extract-plugin#minimizing-for-production
    //      - CopyWebpackPlugin: Copies static assets to output directory
    // DEVELOPMENT
    //      - HotModuleReplacementPlugin: Enables hot reloading when code changes without refreshing
    plugins: (isProduction(env))  ?
        commonPlugins(env).concat([
            new MiniCssExtractPlugin({ filename: 'style.[chunkhash].css' }),
            new CopyWebpackPlugin({
                patterns: [{
                    from: resolve(CONFIG.assetsDir), 
                    globOptions: {
                        ignore: [
                            "**/*.html",
                            "**/*.css",
                        ],
                    }
                }]
            }),
        ])
        : commonPlugins(env).concat([
            new webpack.HotModuleReplacementPlugin(),
        ]),
    resolve: {
            // See https://github.com/fable-compiler/Fable/issues/1490
            symlinks: false,
            alias: {
                './_Pages': path.resolve(__dirname, `src/MyPlanner.Client.View/Pages`),
                './_wwwroot': path.resolve(__dirname, `src/MyPlanner.Client.View/wwwroot`),
                './_Components': path.resolve(__dirname, `src/MyPlanner.Client.View/Components`),
            },
    },
    devServer: {
        publicPath: '/',
        contentBase: resolve(CONFIG.assetsDir),
        host: '0.0.0.0',
        port: CONFIG.devServerPort,
        proxy: CONFIG.devServerProxy,
        hot: true,
        inline:true,
        historyApiFallback: true,
        watchContentBase: true,
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                enforce: "pre",
                use: ["source-map-loader"],
            },
            {
                test: /\.(sass|scss|css)$/,
                use: [
                    isProduction(env)
                        ? MiniCssExtractPlugin.loader
                        : 'style-loader',
                    'css-loader',
                    {
                        loader: 'resolve-url-loader',
                    },
                    {
                        loader: 'sass-loader',
                        options: { implementation: require('sass') }
                    }
                ],
            },
            {
                test: /\.(png|jpg|jpeg|gif|svg|woff|woff2|ttf|eot)(\?.*)?$/,
                use: ['file-loader']
            }
        ]
    }
})

function resolve(filePath) {
    return path.isAbsolute(filePath) ? filePath : path.join(__dirname, filePath);
}
