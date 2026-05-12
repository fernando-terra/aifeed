export default function StatsBar({ total, pages, sourceCount }) {
  return (
    <div style={{
      display: 'flex',
      gap: '32px',
      padding: '16px 24px',
      borderBottom: '1px solid var(--border)',
    }}>
      <Stat value={total != null ? total.toLocaleString() : '—'} label="articles" />
      <Stat value={pages != null ? pages : '—'} label="pages" />
      <Stat value={sourceCount != null ? sourceCount : '—'} label="active sources" />
    </div>
  );
}

function Stat({ value, label }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '2px' }}>
      <div style={{ fontSize: '20px', fontWeight: 700, color: 'var(--text)' }}>{value}</div>
      <div style={{ fontSize: '11px', color: 'var(--muted)', textTransform: 'uppercase', letterSpacing: '0.5px' }}>{label}</div>
    </div>
  );
}
