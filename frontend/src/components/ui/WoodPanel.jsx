import './WoodPanel.css';

export default function WoodPanel({ children, className = '', variant = '', style = {} }) {
  const classes = ['wood-panel', variant ? `wood-panel--${variant}` : '', className]
    .filter(Boolean)
    .join(' ');

  return (
    <div className={classes} style={style}>
      {children}
    </div>
  );
}
