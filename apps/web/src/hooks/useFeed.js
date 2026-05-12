import { useState, useEffect, useCallback, useRef } from 'react';
import { API_BASE } from '../config';

export function useFeed() {
  const [source, setSourceState] = useState('');
  const [page, setPage] = useState(1);
  const [search, setSearchState] = useState('');
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(null);
  const [pages, setPages] = useState(null);
  const [sources, setSources] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [refreshing, setRefreshing] = useState(false);
  const [toast, setToast] = useState(null);
  const searchTimer = useRef(null);

  const showToast = (msg) => {
    setToast(msg);
    setTimeout(() => setToast(null), 3000);
  };

  const loadSources = useCallback(async () => {
    try {
      const res = await fetch(`${API_BASE}/api/sources`);
      const data = await res.json();
      setSources(data);
    } catch (e) {
      console.error('Sources error:', e);
    }
  }, []);

  const loadFeed = useCallback(async (currentSource, currentPage, currentSearch) => {
    setLoading(true);
    setError(null);
    try {
      let url;
      if (currentSearch && currentSearch.trim().length >= 2) {
        url = `${API_BASE}/api/search?q=${encodeURIComponent(currentSearch)}&page=${currentPage}&size=20`;
        if (currentSource) url += `&source=${currentSource}`;
      } else {
        url = `${API_BASE}/api/feed?page=${currentPage}&size=20`;
        if (currentSource) url += `&source=${currentSource}`;
      }
      const res = await fetch(url);
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      const data = await res.json();
      setItems(data.items || []);
      setTotal(data.total);
      setPages(data.pages);
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadSources();
  }, [loadSources]);

  useEffect(() => {
    loadFeed(source, page, search);
  }, [source, page, search, loadFeed]);

  const setSource = useCallback((src) => {
    setSourceState(src);
    setPage(1);
    setSearchState('');
  }, []);

  const setSearch = useCallback((q) => {
    clearTimeout(searchTimer.current);
    searchTimer.current = setTimeout(() => {
      setSearchState(q);
      setPage(1);
    }, 400);
  }, []);

  const refresh = useCallback(async () => {
    setRefreshing(true);
    try {
      const res = await fetch(`${API_BASE}/api/feed/refresh`, { method: 'POST' });
      const data = await res.json();
      showToast('✅ ' + data.message);
      setPage(1);
      await loadFeed(source, 1, search);
    } catch (e) {
      showToast('❌ Refresh failed: ' + e.message);
    } finally {
      setRefreshing(false);
    }
  }, [source, search, loadFeed]);

  return {
    source, setSource,
    page, setPage,
    search, setSearch,
    items, total, pages,
    sources,
    loading, error,
    refreshing, refresh,
    toast,
  };
}
