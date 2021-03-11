var path = require("path");

module.exports = {
  "webpackFinal": async (config) => {
    config.resolve.alias['./_Components'] = path.resolve(__dirname, '../src/MyPlanner.Client.View/Components')
    return config
  },
  
  "stories": [
    "../src/**/*.stories.mdx",
    "../src/**/*.stories.@(js|jsx|ts|tsx)"
  ],
  "addons": [
    "@storybook/addon-links",
    "@storybook/addon-essentials"
  ],
}