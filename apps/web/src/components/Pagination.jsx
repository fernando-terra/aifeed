export default function Pagination({ page, pages, onPage }) {
  if (!pages || pages <= 1) return null;

  return (
    <div style={{
      display: 'flex', alignItems: 'center', justifyContent: 'center',
      gap: '8px', padding: '24px',
    }}>
      <NavBtn disabled={page <= 1} onClick={() => onPage(page - 1)}>← Prev</NavBtn>
      <span style={{ fontSize: '13px', color: 'var(--muted)' }}>Page {page} of {pages}</span>
      <NavBtn disabled={page >= pages} onClick={() => onPage(page + 1)}>Next →</NavBtn>
    </div>
  );
}

function NavBtn({ children, disabled, onClick }) {
  return (
    <button
      disabled={disabled}
      onClick={onClick}
      style={{
        padding: '8px 14px',
        borderRadius: '7px',
        border: '1px solid var(--border)',
        background: 'var(--surface)',
        color: 'var(--text)',
        cursor: disabled ? 'not-allowed' : 'pointer',
        fontSize: '13px',
        fontFamily: 'var(--font)',
        opacity: disabled ? 0.4 : 1,
      }}
    >
      {children}
    </button>
  );
}
