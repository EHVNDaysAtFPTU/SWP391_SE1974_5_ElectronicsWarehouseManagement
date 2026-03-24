# Bootstrap Local Setup

To fix the "Tracking Prevention blocked access to storage" error, Bootstrap CSS and JS files need to be hosted locally instead of using CDN.

## Option 1: Download Bootstrap Files (Recommended)

1. Visit: https://getbootstrap.com/docs/5.3/getting-started/download/
2. Download Bootstrap v5.3.2
3. Extract the zip file
4. Copy `bootstrap.min.css` to: `./css/`
5. Copy `bootstrap.bundle.min.js` to: `./js/`

## Option 2: Using npm (If available)

1. From project root, run:
```bash
npm install bootstrap@5.3.2
```

2. Copy files:
```bash
copy node_modules/bootstrap/dist/css/bootstrap.min.css ./css/
copy node_modules/bootstrap/dist/js/bootstrap.bundle.min.js ./js/
```

## File Structure
```
wwwroot/lib/bootstrap/
├── css/
│   └── bootstrap.min.css
├── js/
│   └── bootstrap.bundle.min.js
└── BOOTSTRAP_SETUP.md (this file)
```

Once files are in place, the local references in itemlist.html will work correctly.
