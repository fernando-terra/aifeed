import { useRef } from 'react';

export default function Toolbar({ onSearch, onRefresh, refreshing }) {
  const inputRef = useRef(null);

  return (
    <div style={{
      display: 'flex',
      alignItems: 'center',
      gap: '12px',
      padding: '16px 24px',
      borderBottom: '1px solid var(--border)',
    }}>
      <div style={{ position: 'relative', flex: 1 }}>
        <span style={{ position: 'absolute', left: '10px', top: '50%', transform: 'translateY(-50%)', color: 'var(--muted)', fontSize: '14px', pointerEvents: 'none' }}>🔍</span>
        <input
          ref={inputRef}
          type="text"
          placeholder="Search articles…"
          onChange={e => onSearch(e.target.value)}
          style={{
            width: '100%',
            padding: '8px 12px 8px 32px',
            background: 'var(--surface)',
            border: '1px solid var(--border)',
            borderRadius: '8px',
            color: 'var(--text)',
            fontSize: '13px',
            fontFamily: 'var(--font)',
            outline: 'none',
          }}
        />
      </div>
      <button
        onClick={onRefresh}
        disabled={refreshing}
        style={{
          padding: '8px 14px',
          borderRadius: '7px',
          border: '1px solid var(--accent)',
          background: 'var(--accent)',
          color: '#fff',
          cursor: refreshing ? 'not-allowed' : 'pointer',
          fontSize: '13px',
          fontFamily: 'var(--font)',
          display: 'flex',
          alignItems: 'center',
          gap: '6px',
          fontWeight: 500,
          opacity: refreshing ? 0.6 : 1,
          transition: 'opacity 0.15s',
          whiteSpace: 'nowrap',
        }}
      >
        ⟳ {refreshing ? 'Refreshing…' : 'Refresh'}
      </button>
    </div>
  );
}
