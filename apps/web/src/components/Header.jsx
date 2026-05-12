export default function Header() {
  return (
    <header style={{
      borderBottom: '1px solid var(--border)',
      padding: '16px 24px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      gap: '16px',
      position: 'sticky',
      top: 0,
      background: 'var(--bg)',
      zIndex: 100,
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '10px', fontWeight: 700, fontSize: '18px', letterSpacing: '-0.3px' }}>
        🤖 <span style={{ color: 'var(--accent)' }}>AI</span>Feed
        <span style={{
          fontSize: '12px', color: 'var(--muted)', fontWeight: 400,
          padding: '2px 8px', border: '1px solid var(--border)', borderRadius: '20px'
        }}>news broker</span>
      </div>
      <div style={{ fontSize: '12px', color: 'var(--muted)', display: 'flex', alignItems: 'center', gap: '6px' }}>
        built with <a href="https://github.com/fernando-terra/arkn" target="_blank" rel="noopener"
          style={{ color: 'var(--accent)', textDecoration: 'none', fontWeight: 500 }}>Arkn</a> · .NET 10
      </div>
    </header>
  );
}
