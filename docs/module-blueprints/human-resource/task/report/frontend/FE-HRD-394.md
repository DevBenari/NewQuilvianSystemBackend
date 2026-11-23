# Laporan Perubahan Frontend — `FE-HRD-394`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-394` |
| Judul | Pensiun halaman Tindak Disiplin lama |
| Slice | `S-PKB` / `MVP-73` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 30 |
| Trace | `FR-PKB-17`; `HRD-DEC-230` butir 2; `HRD-ASM-92`; `AT-HRD-PKB-24` |
| Contract version | `v29`, disetujui 23 November 2026 |
| Wewenang UI | Mengalihkan route lama; tidak mengubah tampilan layar Sanksi |
| Dependency | Tidak ada |
| Klasifikasi | `LIGHT` — satu redirect dan penghapusan dua artefak usang |
| Task mode | `CROSS-REPO` — source frontend dan laporan blueprint backend |
| Target tulis | `QuilvianSystemFrontendDev`; laporan, roadmap, dan traceability HRD |
| Model | Codex GPT-5 |
| Commit frontend saat dikerjakan | `573b7429c967a3229fc651437a902c33eac3708f` |
| Commit backend yang dijadikan rujukan | `b837892ad499289e9dd7dcaf869d9090c50c9939` |
| Tanggal | 23 November 2026 |
| Status | ✅ Selesai |

---

## 1. Keadaan yang ditemukan di awal

Route `/hr/employee-relations/disciplinary-actions` masih merender layar lama dan memakai `employee-relations-service.js` untuk memanggil `/v1/hr/workforce-profile/disciplinary-actions`, sedangkan endpoint tersebut tidak tersedia. Layar Sanksi resmi sudah tersedia pada `/hrd/relations/sanctions` dan memakai service berbeda, `employee-relation-service.js`.

---

## 2. Proses bisnis dari sisi pengguna

Pengguna yang membuka bookmark atau tautan lama Tindak Disiplin sekarang langsung dialihkan ke layar Surat Peringatan/Sanksi yang aktif. Pengguna tidak lagi melihat layar kosong atau galat akibat endpoint lama. Hak akses, keadaan kosong, gagal, dan pemuatan selanjutnya tetap ditangani layar Sanksi tujuan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `src/app/hr/employee-relations/disciplinary-actions/page.jsx`
- `src/components/view/hr/employee-relations/disciplinary-action-view.jsx`
- `src/lib/services/hr/employee-relations-service.js`
- `src/lib/services/hr/employee-relation-service.js`
- `src/app/(dashboard)/hrd/relations/sanctions/page.jsx`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/app/hr/employee-relations/disciplinary-actions/page.jsx` | Mengganti render layar lama dengan `redirect("/hrd/relations/sanctions")`. |
| `src/components/view/hr/employee-relations/disciplinary-action-view.jsx` | Dihapus karena tidak memiliki pemakai setelah redirect. |
| `src/lib/services/hr/employee-relations-service.js` | Dihapus agar endpoint lama tidak dapat dipanggil lagi. |
| `tests/e2e/human-resource-package-b.spec.mjs` | Menambahkan skenario browser headed dan screenshot redirect. |

### 3.3 Kepatuhan arsitektur frontend

Route memakai `redirect` milik Next.js App Router, sama dengan pola route pengalihan lain dalam proyek. Service Sanksi aktif `employee-relation-service.js` tidak diubah. Tidak ada dependency atau komponen baru.

### 3.4 Gerbang tampilan

`NOT APPLICABLE` — task tidak mengubah JSX tampilan atau CSS; ia hanya mengalihkan route ke layar existing.

`UI GATE: PASS — 1 REUSE pola redirect, 0 COMPOSE, 0 NEW.`

| Layar | Screenshot | Catatan perbandingan dengan referensi |
| --- | --- | --- |
| `/hr/employee-relations/disciplinary-actions` → `/hrd/relations/sanctions` | `test-with-agy/hr-paket-b-1123/FE-HRD-394-redirect.png` | Route tujuan menampilkan heading "Surat Peringatan". Screenshot dibuat ulang 23 November 2026 saat regresi seluruh spec Paket B (15/15, laporan `FE-HRD-400`); salinan lama di `test-results/` terhapus oleh run Playwright berikutnya. |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Ditangani layar Sanksi tujuan. |
| Kosong | Ditangani layar Sanksi tujuan. |
| Gagal | Ditangani layar Sanksi tujuan; endpoint usang tidak lagi dipanggil. |
| Tanpa hak akses | Ditangani pemeriksaan hak akses layar Sanksi tujuan. |

---

## 5. Endpoint yang dikonsumsi

`NOT APPLICABLE` — route pengalihan tidak memanggil API. Endpoint dan hak akses layar tujuan tidak diubah oleh task ini.

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | Selesai dengan exit code `0` | `PASS` | ESLint seluruh repository, tanpa error |
| `npx eslint ...page.jsx ...human-resource-package-b.spec.mjs --quiet` | Exit code `0` | `PASS` | Validasi berkas task setelah spec ditambahkan |
| `npm run build` | Exit code `0`; 890 halaman statis dihasilkan | `PASS` | Next.js production build dan standalone preparation berhasil |
| Pencarian endpoint/service usang | Tidak ada rujukan tersisa pada `src` | `PASS` | `rg` untuk `employee-relations-service` dan `/v1/hr/workforce-profile/disciplinary-actions` |
| Playwright Chromium headed | 1 skenario lulus | `PASS` | `FE-HRD-394` mengarah ke `/hrd/relations/sanctions` dan heading terlihat |

Uji manual: `PASS` melalui browser Chromium headed terotomasi.

**Tidak dijalankan:** API backend nyata tidak diperlukan karena task hanya menguji redirect dan layar tujuan dimock pada jaringan `/v1/**`.

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Membuka alamat lama membawa pengguna ke layar Sanksi | Terpenuhi | Playwright headed dan screenshot `FE-HRD-394-redirect.png` |
| Source tidak merujuk service maupun endpoint lama | Terpenuhi | Dua artefak usang dihapus dan pencarian source kosong |
| `employee-relation-service.js` aktif tidak terhapus/berubah | Terpenuhi | Berkas tetap ada dan tidak tercantum dalam diff task |
| Lint, build, browser test, laporan, roadmap, dan traceability | Terpenuhi | Bukti bagian 6 dan pembaruan dokumen terkait |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Chromium Playwright perlu dipasang sekali pada mesin; pemasangan berhasil sebelum uji diulang. |
| Masalah yang diketahui | `NONE` |
| Dependency backend | `NOT APPLICABLE` |
| Perubahan sampingan | Spec Paket B dibuat sebagai wadah uji browser task berikutnya. |
| Interupsi | Percobaan browser pertama gagal karena executable belum terpasang; dipulihkan dengan `npx playwright install chromium`. |
| Status Git | Source task dan dokumen Paket B belum di-commit; artefak laporan Playwright lokal ikut berubah setelah uji. |
| Langkah berikutnya | Kerjakan `FE-HRD-395` sesuai gelombang 1. |
