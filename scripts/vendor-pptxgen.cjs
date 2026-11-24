const fs = require('node:fs');
const path = require('node:path');

const distDirectory = path.dirname(require.resolve('pptxgenjs'));
const projectDirectory = path.resolve(__dirname, '..');

fs.copyFileSync(
  path.join(distDirectory, 'pptxgen.bundle.js'),
  path.join(projectDirectory, 'vendor', 'pptxgen.bundle.js')
);
fs.copyFileSync(
  path.join(distDirectory, '..', 'LICENSE'),
  path.join(projectDirectory, 'licenses', 'PptxGenJS-LICENSE.txt')
);
