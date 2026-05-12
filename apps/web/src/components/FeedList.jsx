import FeedItem from './FeedItem';

function Spinner() {
  return (
    <div style={{
      display: 'flex', flexDirection: 'column', alignItems: 'center',
      justifyContent: 'center', padding: '80px 24px', gap: '12px',
      color: 'var(--muted)', fontSize: '14px',
    }}>
      <div style={{
        width: '28px', height: '28px',
        border: '3px solid var(--border)',
        borderTopColor: 'var(--accent)',
        borderRadius: '50%',
        animation: 'spin 0.7s linear infinite',
      }} />
      <style>{`@keyframes spin { to { transform: rotate(360deg); } }`}</style>
      <span>Loading…</span>
    </div>
  );
}

function StateBox({ icon, message }) {
  return (
    <div style={{
      display: 'flex', flexDirection: 'column', alignItems: 'center',
      justifyContent: 'center', padding: '80px 24px', gap: '12px',
      color: 'var(--muted)', fontSize: '14px',
    }}>
      <div style={{ fontSize: '40px' }}>{icon}</div>
      <span>{message}</span>
    </div>
  );
}

export default function FeedList({ items, loading, error }) {
  if (loading) return <Spinner />;
  if (error) return <StateBox icon="⚠️" message={`Could not load feed: ${error}`} />;
  if (!items || items.length === 0) return <StateBox icon="📭" message="No items found." />;

  return (
    <div style={{ padding: '20px 24px', display: 'flex', flexDirection: 'column', gap: '10px' }}>
      {items.map(item => <FeedItem key={item.id} item={item} />)}
    </div>
  );
}
