import './index.css';
import { useFeed } from './hooks/useFeed';
import Header from './components/Header';
import Sidebar from './components/Sidebar';
import StatsBar from './components/StatsBar';
import Toolbar from './components/Toolbar';
import FeedList from './components/FeedList';
import Pagination from './components/Pagination';

export default function App() {
  const {
    source, setSource,
    page, setPage,
    setSearch,
    items, total, pages,
    sources,
    loading, error,
    refreshing, refresh,
    toast,
  } = useFeed();

  const handlePageChange = (p) => {
    setPage(p);
    window.scrollTo({ top: 0 });
  };

  return (
    <>
      <Header />

      <div style={{
        display: 'grid',
        gridTemplateColumns: '220px 1fr',
        minHeight: 'calc(100vh - 57px)',
      }}>
        <Sidebar sources={sources} activeSource={source} onSelect={setSource} />

        <main>
          <StatsBar total={total} pages={pages} sourceCount={sources.length || null} />
          <Toolbar onSearch={setSearch} onRefresh={refresh} refreshing={refreshing} />
          <FeedList items={items} loading={loading} error={error} />
          <Pagination page={page} pages={pages} onPage={handlePageChange} />
        </main>
      </div>

      {toast && (
        <div style={{
          position: 'fixed', bottom: '24px', right: '24px',
          background: 'var(--surface)',
          border: '1px solid var(--border)',
          borderRadius: '8px',
          padding: '10px 16px',
          fontSize: '13px',
          display: 'flex',
          alignItems: 'center',
          gap: '8px',
          zIndex: 200,
          boxShadow: '0 4px 24px #00000060',
        }}>
          {toast}
        </div>
      )}
    </>
  );
}
