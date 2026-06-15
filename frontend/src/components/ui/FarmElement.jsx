import './FarmElement.css';

export default function FarmElement({ spriteUrl, label, top, left, onClick }) {
  return (
    <div
      className="farm-element"
      style={{ top, left }}
      onClick={onClick}
      role="button"
      tabIndex={0}
      onKeyDown={(e) => e.key === 'Enter' && onClick?.()}
      aria-label={label}
    >
      <div className="farm-element__tooltip">{label}</div>
      <img
        src={spriteUrl}
        alt={label}
        className="farm-element__sprite"
        draggable={false}
      />
      <span className="farm-element__label">{label}</span>
    </div>
  );
}
