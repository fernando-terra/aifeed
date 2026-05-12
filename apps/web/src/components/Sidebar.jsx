const SOURCE_META = {
  hackernews:  { label: 'Hacker News',  color: 'hackernews'  },
  devto:       { label: 'dev.to',        color: 'devto'       },
  arxiv:       { label: 'arXiv',         color: 'arxiv'       },
  github:      { label: 'GitHub',        color: 'github'      },
  producthunt: { label: 'Product Hunt',  color: 'producthunt' },
};

export default function Sidebar({ sources, activeSource, onSelect }) {
  return (
    <aside style={{
      borderRight: '1px solid var(--border)',
      padding: '24px 16px',
      display: 'flex',
      flexDirection: 'column',
      gap: '8px',
    }}>
      <span style={{ fontSize: '10px', textTransform: 'uppercase', letterSpacing: '1px', color: 'var(--muted)', padding: '4px 10px', marginTop: '8px' }}>
        Sources
      </span>
      <SourceBtn label="All sources" color="all" active={activeSource === ''} onClick={() => onSelect('')} />
      {sources.map(s => {
        const meta = SOURCE_META[s.id] || { label: s.displayName, color: 'all' };
        return (
          <SourceBtn key={s.id} label={meta.label} color={meta.color} active={activeSource === s.id} onClick={() => onSelect(s.id)} />
        );
      })}
    </aside>
  );
}

function SourceBtn({ label, color, active, onClick }) {
  return (
    <button onClick={onClick} style={{
      display: 'flex',
      alignItems: 'center',
      gap: '10px',
      padding: '8px 12px',
      borderRadius: '8px',
      border: active ? '1px solid var(--accent)' : '1px solid transparent',
      background: active ? 'var(--accent-dim)' : 'transparent',
      color: active ? 'var(--text)' : 'var(--muted)',
      cursor: 'pointer',
      fontSize: '13px',
      fontFamily: 'var(--font)',
      fontWeight: active ? 500 : 400,
      transition: 'all 0.15s',
      textAlign: 'left',
    }}>
      <span className={`c-${color}`} style={{ width: '10px', height: '10px', borderRadius: '50%', flexShrink: 0, display: 'inline-block' }} />
      {label}
    </button>
  );
}
