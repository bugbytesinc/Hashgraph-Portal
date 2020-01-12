module.exports = [{
    entry: './src/main.scss',
    module: {
      rules: [
        {
          test: /\.scss$/,
          use: [
            {
              loader: 'file-loader',
              options: {
                name: '../../Hashgraph.Portal/wwwroot/main.css',
              },
            },
            { loader: 'extract-loader' },
            { loader: 'css-loader' },
            { loader: 'sass-loader' },
          ]
        }
      ]
    },
  }];