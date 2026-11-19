# LGS-FE-ADH-02 — Layar persetujuan permintaan barang unit (LGS-DEC-002)

- TASK ID: `LGS-FE-ADH-02` (task ad-hoc). Mencakup FE-01 sampai FE-05 pada instruksi pemrakarsa "Instruksi Eksekusi Agent — LGS-OQ-004", 17 November 2026
- DEPENDENCY: `LGS-BE-ADH-02` (source selesai; belum diverifikasi runtime)
- TASK MODE: FRONTEND (dinyatakan eksplisit oleh pemrakarsa)
- WRITE TARGET: `QuilvianSystemFrontendDev`, branch `Ikbalv2`, HEAD `1e9adc790`
- MODEL: Claude Opus 5.5
- WEWENANG UI: skema tampilan FE-05 dari instruksi pemrakarsa; komponen base yang sudah ada (`Hero`, `DataFilter`, `FilterSelect`, `DataTable`, `StatusBadge`, `InformationAlert`, `BaseButton`, `ConfirmModal`, `AccessDeniedGate`)

## File

Baru:

- `src/utils/health-services/pharmacy-management/stock-request-approval-utils.js`: aturan tampilan yang murni (tanpa React), sehingga bisa diuji tanpa build
- `src/lib/hooks/health-services/pharmacy-management/use-stock-request-approval-summary.jsx`: empat penghitung dari `totalData` endpoint daftar
- `src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-decision-panel.jsx`: Setujui, Tolak, Otorisasi CITO
- `src/components/view/health-services/pharmacy-management/stock-request-view/components/stock-request-submit-panel.jsx`: Ajukan Persetujuan, termasuk jalur CITO
- `src/app/health-services/pharmacy-management/stock-requests/approvals/page.jsx` dan `stock-request-approval-client.jsx`
- `src/app/health-services/pharmacy-management/stock-requests/approved/page.jsx` dan `stock-request-approved-queue-client.jsx`
- `tests/unit/stock-request-approval.test.mjs`: 12 test

Diubah:

- `src/lib/constants/health-services/pharmacy-management/stock-request-constant.jsx`: label "Menunggu Persetujuan", varian badge, label "Dipenuhi Sebagian", label aksi riwayat, label prioritas "CITO (Gawat Darurat)", konstanta route
- `src/lib/services/health-services/pharmacy-management/stock-request.service.js`: `approveStockRequest`, `rejectStockRequest`, `authorizeEmergencyStockRequest`
- `src/lib/hooks/health-services/pharmacy-management/use-stock-request-detail.jsx`: `submit(emergencyReason)`, `approve`, `reject`, `authorizeEmergency`
- `src/lib/hooks/health-services/pharmacy-management/use-stock-request-list.jsx`: opsi `initialStatus` dan `orderByPriority`
- `src/components/view/.../stock-request-view/stock-request-list-view.jsx`: tiga varian (`history`, `approval`, `warehouse`)
- `src/components/view/.../stock-request-view/stock-request-detail-view.jsx`: urutan Info → Barang → Alasan → Histori → Aksi
- `src/components/view/.../stock-request-view/stock-request-form-view.jsx`: petunjuk saat prioritas CITO dipilih
- `src/components/view/.../stock-request-view/components/stock-request-warehouse-panel.jsx`: komentar saja
- `src/utils/menu-sidebar/menu-items.jsx`: tautan "Validasi Atasan" dan "Daftar Permintaan Disetujui" (CRLF dipertahankan)
- `tests/unit/logistics-module-navigation.test.mjs`: batas minimum tautan 22 → 24

## Perilaku

| Task | Implementasi |
| --- | --- |
| FE-01 Daftar persetujuan | Route `/stock-requests/approvals`, menu "Validasi Atasan". Header → ringkasan (Menunggu Persetujuan, CITO Menunggu, Disetujui Belum Diserahkan, Ditolak) → aturan → filter (status, prioritas, obat, unit, gudang, tanggal, pencarian) → tabel berhalaman. Dibuka pada status Menunggu Persetujuan, CITO paling atas. Loading, kosong, error dengan Coba lagi, dan 403 lewat `AccessDeniedGate` |
| FE-02 Detail dan aksi | Panel Keputusan setelah Riwayat. Setujui (catatan opsional), Tolak (alasan wajib), Otorisasi CITO (alasan wajib). Langkahnya: pilih → isi → Lanjutkan ke Konfirmasi → `ConfirmModal`. Pemohon melihat penjelasan, bukan tombol. Tanpa izin: penjelasan "tidak memiliki kewenangan" |
| FE-03 Form CITO | Form menampilkan petunjuk saat CITO dipilih. Di detail, "Ajukan Persetujuan" untuk CITO membuka penanda CITO → alasan wajib → ringkasan jumlah item → konfirmasi → status Menunggu Persetujuan |
| FE-04 Tampilan gudang | Route `/stock-requests/approved`, menu "Daftar Permintaan Disetujui", hanya status Disetujui, CITO dulu. Panel penyerahan hanya muncul bila backend mengirim `canFulfill`, yang kini hanya benar untuk Disetujui |
| FE-05 Skema | Halaman, detail, dan form CITO mengikuti urutan pada instruksi |

Izin di layar:

- `StockRequest:Approve`: memakai `usePermission` biasa. Selama daftar izin belum termuat, tombol tampil; backend tetap menolak dengan 403. Ini pola yang sama dengan layar lain.
- `StockRequest:AuthorizeEmergency`: memakai keputusan ketat. Tombol baru tampil setelah daftar izin termuat dan izinnya ada.
- Layar tidak pernah mengirim status. Hasil keputusan ditentukan backend.

## Validasi

| Pemeriksaan | Hasil | Catatan |
| --- | --- | --- |
| `npm run build`, `npm run dev` | NOT RUN | Dilarang instruksi pemrakarsa |
| ESLint pada file yang diubah | PASS, 0 error | Warning `react-hooks/set-state-in-effect` pada `use-stock-request-detail`, `use-stock-request-list` (sudah ada di HEAD) dan `use-stock-request-approval-summary` (pola yang sama). Warning `preserve-manual-memoization` di `stock-request-resource-select.jsx` sudah ada sebelumnya; file itu tidak diubah |
| `tests/unit/stock-request-approval.test.mjs` | PASS 12/12 | Aturan pemohon, izin, CITO, status final, alasan wajib, label, kontrak panggilan, urutan detail, route, menu |
| `tests/unit/logistics-module-navigation.test.mjs` | PASS 8/8 | 24 tautan, semua menunjuk `page.jsx` yang ada |
| Test yang menyentuh menu, stok, dan permintaan (50 file, 590 test) | 23 gagal, sama persis dengan baseline sebelum perubahan | Tidak ada kegagalan baru |
| Seluruh `tests/unit` (483 file) | 85 gagal | 62 di luar baseline lama berada di modul lain (Billing, HR, Rawat Inap, IGD, seeder backend). Tidak satu pun membaca file yang diubah task ini; baseline lama hanya mencakup 50 file di atas |

- MANUAL TEST: NOT FEASIBLE. Dev server dan build dilarang. Belum diverifikasi: tampilan modal, filter gabungan, paginasi, dan respons 403/409 sungguhan.

## Risiko

1. Backend belum dikompilasi; layar ini bergantung pada `LGS-BE-ADH-02`.
2. Tombol Kembali memakai riwayat browser. Bila detail dibuka langsung, kembali ke Riwayat Permintaan.
3. Ringkasan memanggil endpoint daftar empat kali (`pageSize=1`). Ringan, tetapi bukan endpoint ringkasan khusus.
4. Di mesin pengembang ini pemeriksaan izin backend dimatikan (`appsettings.Development.json`), sehingga uji manual izin perlu konfigurasi dengan pemeriksaan aktif.

- GIT STATUS: belum di-commit
- NEXT RECOMMENDED STEP: jalankan dev server, lalu uji tiga peran (pemohon, atasan berizin, petugas CITO berizin) pada dua halaman baru dan halaman detail
