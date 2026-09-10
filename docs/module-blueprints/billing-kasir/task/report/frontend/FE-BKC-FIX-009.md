# Laporan Perubahan Frontend — `FE-BKC-FIX-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-BKC-FIX-009` (ad-hoc, di luar roadmap, permintaan langsung pengguna) |
| Judul | Info "Co-payment pasien: Rp{nominal}" per baris item Menu Pembayaran, untuk item berbadge "Penjamin" yang coverage-nya tidak 100% |
| Slice | Lanjutan investigasi live invoice Allianz, setelah `BE-BKC-FIX-005` |
| Roadmap | `NOT APPLICABLE` |
| Trace | `NOT APPLICABLE` |
| Contract version | `NOT APPLICABLE` — mengonsumsi field yang sudah ada di payload `GET .../calculation-preview` sejak `BE-BKC-FIX-003` (`netAmount`, `itemPrimaryAmount`, `itemUnresolvedAmount`, `taxPrimaryAmount`, `taxUnresolvedAmount`) |
| Wewenang UI | `REUSE` murni — `styles.sectionHint` (sudah ada di `menu-pembayaran.module.css`, dipakai untuk hint di bawah judul section) dipakai ulang apa adanya, tidak ada CSS/token baru |
| Dependency | `BE-BKC-FIX-003` (backend, sudah live) |
| Klasifikasi | `LOW` — murni tampilan tambahan (informational), tidak mengubah kalkulasi/badge/urutan yang sudah ada |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `menu-pembayaran-view.jsx` |
| Model | Claude Sonnet 5 |
| Tanggal | 4 September 2026 |
| Status | Source selesai, lint bersih. **Belum diverifikasi hidup** — dev server (Turbopack HMR) belum memuat ulang perubahan (pola staleness yang sama seperti task-task sebelumnya di sesi ini) |

---

## 1. Keadaan yang ditemukan di awal

Pengguna baru saja mengubah konfigurasi rule "Coverage Allianz Kategori Radiologi" (CoveragePercent
75%, CoPaymentPercent 25%) dan bertanya kenapa item Radiology masih menyisakan Subtotal Mandiri
padahal badge-nya "Penjamin". Setelah saya verifikasi lewat query backend langsung, angka Subtotal
Mandiri/Asuransi TERNYATA sudah benar secara matematis mengikuti aturan co-payment rule itu — badge
"Penjamin" cuma menyatakan "rule asuransi ADA dan berlaku", bukan "seluruh nominal item ditanggung
penuh". Tidak ada cara bagi kasir untuk tahu, hanya dari badge, bahwa sebagian nominal tetap jadi
tanggungan pasien akibat co-payment/coverage <100%.

## 2. Proses bisnis dari sisi pengguna

**Pengguna**: kasir di Menu Pembayaran.

**Sebelum**: badge "Penjamin" tidak membedakan antara "ditanggung 100%" dan "ditanggung sebagian,
sisanya co-payment pasien" — berpotensi disalahpahami kasir/pasien.

**Sesudah**: di bawah badge "Penjamin", muncul baris kecil "Co-payment pasien: Rp{nominal}" HANYA
ketika item itu benar-benar mendapat coverage asuransi (bukan 0) TAPI masih menyisakan porsi ke
pasien (co-payment persentase/nominal, atau coverage <100%). Item yang coverage-nya 100% (mis.
Administration, Drug pada invoice contoh) tidak menampilkan baris ini sama sekali.

**Jalur tidak normal**: item berbadge "Menunggu Verifikasi" SENGAJA tidak menampilkan info ini —
porsi yang belum diverifikasi bukan co-payment permanen (masih berpotensi berubah jadi tanggungan
asuransi penuh begitu diverifikasi), jadi tidak dilabeli seolah sudah pasti jadi beban pasien.

---

## 3. Perubahan yang dikerjakan

### 3.1 `menu-pembayaran-view.jsx`

- `itemOutcomeById` (map per item dari `breakdown.items[]`) ditambah field `total` (`netAmount`/
  `NetAmount` — total item termasuk pajak).
- Helper baru `getItemCoPayInfo(item)`: mengambil outcome item, menghitung
  `covered = itemPrimary + taxPrimary`, `unresolvedTotal = itemUnresolved + taxUnresolved`, lalu
  `residual = total - covered - unresolvedTotal`. Mengembalikan `residual` HANYA bila
  `unresolvedTotal === 0 && covered > 0 && residual > 0.5` (toleransi pembulatan) — kondisi ini
  sengaja dibuat SAMA PERSIS dengan cabang `"penjamin"` di `getItemCoverageStatus` supaya info ini
  tidak pernah muncul untuk item berstatus lain.
- Baris tabel item: `coPayAmount = getItemCoPayInfo(item)` dihitung sekali per item, dirender
  sebagai `<span className={styles.sectionHint}>Co-payment pasien: {formatMoney(coPayAmount)}</span>`
  di bawah `<StatusBadge>` dalam sel Status yang sama.

**Tabel keputusan base component:**

| Elemen | Keputusan | Alasan |
| --- | --- | --- |
| Baris hint "Co-payment pasien" | `REUSE` | `styles.sectionHint` sudah ada di file CSS modul yang sama (`menu-pembayaran.module.css` baris 61-71), dipakai apa adanya tanpa nilai visual literal baru |

**`UI GATE`**: tidak ada — murni REUSE, tidak ada base component baru/extend.

---

## 4. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint --quiet menu-pembayaran-view.jsx` | Berhasil tanpa error | `PASS` |
| Verifikasi manual rumus vs data live (Playwright, invoice IKBAL YULIYANTO, item Radiology) | `getItemCoPayInfo` dihitung manual: net=333000, itemPrimary=166500, itemUnresolved=0, taxPrimary=0 → residual=166500 → HARUSNYA render "Co-payment pasien: Rp166.500" | `PASS` (analisis kode) |
| Uji hidup di browser (Playwright, halaman Menu Pembayaran invoice yang sama) | Teks "Co-payment pasien" **TIDAK muncul** di HTML yang dirender — dicek ulang, source file di disk SUDAH benar berisi perubahan ini (`grep` mengonfirmasi baris 519 ada), jadi ini staleness dev server (Turbopack HMR belum reload), BUKAN bug source | `MANUAL TEST: BLOCKED (dev server stale)` |

**Tidak dijalankan:** component test (instruksi eksplisit pengguna sepanjang sesi ini — tanpa file
test).

---

## 5. Temuan sampingan (di luar scope task ini — dilaporkan, tidak diperbaiki tanpa arahan pengguna)

Saat memverifikasi angka co-payment Radiology dengan rule BARU pengguna (CoveragePercent=75,
CoPaymentPercent=25, CoPaymentAmount=0), ditemukan `CalculateCoveredAmount()`
(`BillingCoverageAdapter.cs`) MENUMPUK kedua persentase itu SECARA TERPISAH, bukan memperlakukannya
sebagai pasangan pelengkap (75%+25%=100%):

```
eligible = 333.000
covered  = eligible * 75% = 249.750         (CoveragePercent)
covered -= eligible * 25% = 249.750 - 83.250 = 166.500   (CoPaymentPercent, dikurangi LAGI dari eligible)
```

Hasil akhir: asuransi menanggung **166.500 dari 333.000 (persis 50%)**, BUKAN 75% seperti yang
mungkin dimaksud pengguna saat mengisi `CoveragePercent=75`. Ini murni pertanyaan interpretasi
bisnis (bukan bug yang saya perbaiki tanpa arahan): apakah `CoveragePercent` dan `CoPaymentPercent`
memang DIMAKSUDKAN sebagai dua pengurang independen yang bisa ditumpuk (co-payment "di atas"
coverage), atau seharusnya keduanya jadi pasangan yang saling melengkapi (isi salah satu saja,
atau kalau diisi keduanya harus konsisten menjumlah 100% dan hasilnya `eligible * CoveragePercent%`
saja, tanpa pengurangan kedua). Dilaporkan ke pengguna langsung di chat, menunggu keputusan sebelum
kode `CalculateCoveredAmount()` disentuh.

---

## 6. Risiko dan catatan penutup

| Hal | Isi |
| --- | --- |
| Belum diverifikasi hidup | Menunggu dev server (Turbopack) reload perubahan — pola staleness yang sama seperti beberapa task sebelumnya di sesi ini, bukan indikasi bug |
| Perubahan sampingan | `NONE`. Tidak ada file test dibuat |
| Status Git | Modified: `menu-pembayaran-view.jsx`. Belum staged/commit |
| Langkah berikutnya | Restart/reload frontend dev server, verifikasi ulang baris "Co-payment pasien: Rp166.500" muncul di bawah badge "Penjamin" item Radiology; putuskan temuan sampingan § 5 (stacking CoveragePercent/CoPaymentPercent) sebelum menyentuh `CalculateCoveredAmount()` lebih lanjut |

