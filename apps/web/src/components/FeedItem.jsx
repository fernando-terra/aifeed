const SOURCE_META = {
  hackernews:  { label: 'Hacker News',  color: 'hackernews'  },
  devto:       { label: 'dev.to',        color: 'devto'       },
  arxiv:       { label: 'arXiv',         color: 'arxiv'       },
  github:      { label: 'GitHub',        color: 'github'      },
  producthunt: { label: 'Product Hunt',  color: 'producthunt' },
};

function timeAgo(iso) {
  const diff = Date.now() - new Date(iso).getTime();
  const m = Math.floor(diff / 60000);
  if (m < 1)  return 'just now';
  if (m < 60) return `${m}m ago`;
  const h = Math.floor(m / 60);
  if (h < 24) return `${h}h ago`;
  return `${Math.floor(h / 24)}d ago`;
}

export default function FeedItem({ item }) {
  const meta = SOURCE_META[item.source] || { label: item.source, color: 'all' };
  const date = item.publishedAt ? timeAgo(item.publishedAt) : '';
  const tags = (item.tags || []).slice(0, 3);

  return (
    <div style={{
      background: 'var(--surface)',
      border: '1px solid var(--border)',
      borderRadius: 'var(--radius)',
      padding: '16px 18px',
      display: 'flex',
      gap: '16px',
    }}>
      <span className={`c-${meta.color}`} style={{ width: '10px', height: '10px', borderRadius: '50%', flexShrink: 0, marginTop: '5px', display: 'inline-block' }} />
      <div style={{ flex: 1, minWidth: 0 }}>
        <div style={{ fontSize: '14px', fontWeight: 500, lineHeight: 1.5, marginBottom: '6px' }}>
          <a href={item.url} target="_blank" rel="noopener noreferrer"
            style={{ color: 'var(--text)', textDecoration: 'none' }}
            onMouseEnter={e => e.target.style.color = 'var(--accent)'}
            onMouseLeave={e => e.target.style.color = 'var(--text)'}
          >
            {item.title}
          </a>
        </div>
        {item.summary && (
          <div style={{
            fontSize: '12px', color: 'var(--muted)', lineHeight: 1.6, marginBottom: '8px',
            display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical', overflow: 'hidden',
          }}>
            {item.summary}
          </div>
        )}
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px', flexWrap: 'wrap' }}>
          <span className={`t-${meta.color}`} style={{
            fontSize: '10px', textTransform: 'uppercase', letterSpacing: '0.5px', fontWeight: 600,
            padding: '2px 7px', borderRadius: '4px', background: 'var(--tag-bg)',
          }}>{meta.label}</span>
          {item.score > 0 && (
            <span style={{ fontSize: '11px', color: 'var(--muted)', display: 'flex', alignItems: 'center', gap: '4px' }}>
              ⬆ {item.score.toLocaleString()}
            </span>
          )}
          {date && <span style={{ fontSize: '11px', color: 'var(--muted)' }}>{date}</span>}
          {item.author && <span style={{ fontSize: '11px', color: 'var(--muted)' }}>· {item.author}</span>}
          {tags.map(t => (
            <span key={t} style={{
              fontSize: '10px', padding: '2px 7px', borderRadius: '4px',
              background: 'var(--tag-bg)', color: 'var(--muted)', border: '1px solid var(--border)',
            }}>{t}</span>
          ))}
        </div>
      </div>
    </div>
  );
}
