# Catatan untuk Owner Billing — Hasil Uji Tagihan IGD 18 Oktober 2026

| Field | Value |
|---|---|
| `request_id` | `IGD-REQ-006` |
| `tanggal` | 2026-10-18 |
| `pengaju` | Rizki Gunawan — Product/Domain Owner IGD (`IGD-DEC-089`) |
| `penerima` | **Owner Billing/Kasir** (blueprint `billing-kasir`); tembusan pemilik Farmasi untuk butir 6 |
| `rujukan` | `IGD-DEC-242`, `250`, `251`, `253`, `326`; `IGD-FACT-147`, `150`, `151`, `152`, `153`; `IGD-OQ-129`, `IGD-OQ-137`; Billing `BKC-DEC-117`, `118`, `139`, `BE-BKC-FIX-016`, `019`; [penilaian uji K1](../testing/2026-10-18-laporan-uji-putaran-k1.md); [catatan bukti uji](../evidence/2026-10-18-catatan-bukti-uji-p1-k1-dan-serah-terima.md) |
| `status` | **`menunggu jawaban`** |
| `sifat` | Operasional, lintas modul. **Bukan** artefak desain IGD. Tidak meminta perubahan kode Billing |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Uji tagihan IGD tanggal 18 Oktober 2026 (putaran K1, dinilai dari log backend) membuktikan jalur yang dibuka Billing untuk
IGD (`BE-BKC-FIX-016`, `019`) **bekerja**: jasa kunjungan IGD, tindakan perawat, dan obat masuk ke satu invoice IGD per
kunjungan untuk pasien tunai, BPJS, dan PKS, tanpa baris yang tertahan di rekonsiliasi. Ada **lima hal yang perlu
keputusan atau konfirmasi Billing** sebelum demo Kasir IGD Rabu 21 Oktober — terutama porsi penjamin yang belum terhitung
dan harga jasa kunjungan yang masih nilai uji. Tidak ada permintaan perubahan kode.

_Contoh dari data uji._ Pasien K1-B (BPJS, _Cashless_) mendapat invoice `BIL-20261018-00000003`: jasa kunjungan Rp150.000,
infus Rp50.000, obat Rp65.000 — bruto Rp265.000. Daftar invoice mengenali penjaminnya (`INSURANCE`, BPJS Kesehatan),
tetapi ringkasan tagihan masih menunjukkan pasien membayar Rp265.000 dan penjamin Rp0 (`currentCalculationVersion` 0).

---

## 2. Yang sudah terbukti — tidak perlu ditindaklanjuti

| Hal | Bukti (log backend 18 Oktober 2026, WIB) |
|---|---|
| Jasa kunjungan IGD terbit saat DPJP pertama ditetapkan dan masuk invoice | 19.34.38, 19.34.51, 19.35.04 — `EmergencyVisitFee.Recorded` → `PatientProcedure.EnsureCompletedFact` → `ClinicalFact.Dispatch` → `BillingBridge.Sync` → `BillingInvoice.CreateCharge` dalam satu permintaan |
| Tidak ganda saat DPJP dialihkan | Pengalihan 20.07.32; tetap satu baris |
| Tindakan perawat dan obat IGD masuk invoice IGD | Tindakan perawat 19.36.58–19.37.20; resep 19.53.22–19.56.31 |
| Antrean rekonsiliasi untuk kunjungan uji kosong | `GET …/charge-reconciliations?encounterId=…` — 0 baris |
| Invoice IGD pasien transfer berdiri sendiri (`IGD-DEC-293`) | `BIL-20261018-00000005` Rp200.000, `OPEN`, tunai |

---

## 3. Yang diminta dari Billing

| No | Hal | Keadaan hari ini | Yang diminta |
|---|---|---|---|
| 1 | **Harga dan kategori _Jasa kunjungan IGD_** | Item `PR-RSMMC-00025` dan tarif `TF-RSMMC-00016` dibuat tim IGD di dev 18 Oktober 2026 (`IGD-FACT-145`): Rp150.000, kategori _Consultation_, tanpa klinik dan kelas, berlaku sejak 17 Oktober 2026. **Nilai uji** (contoh `IGD-DEC-250`), bukan tarif rumah sakit | Tetapkan harga, kategori, dan apakah per kelas pasien. Item sengaja tidak ditawarkan di pemilih tindakan supaya tidak dipilih manual |
| 2 | **Porsi penjamin pada invoice IGD** | `primaryPayerType` dikenali, tetapi `primaryAmount` 0 dan `currentCalculationVersion` 0 untuk BPJS dan PKS (`IGD-FACT-152`) | Konfirmasi kapan porsi penjamin dihitung (mis. saat kasir menekan hitung) dan apa yang perlu disiapkan supaya demo 21 Oktober menampilkan bagian penjamin dan bagian pasien |
| 3 | **Biaya administrasi IGD Rp0** | Ketiga invoice uji `administrationFeeAmount` 0 | Konfirmasi apakah kebijakan administrasi untuk jenis layanan `IGD` memang belum diisi di dev, atau IGD memang tanpa biaya administrasi |
| 4 | **Dua tarif obat yang dibuat penguji** | Di tengah uji, akun SuperAdmin membuat dua tarif obat (id `c43d2bbf-3784-42ef-83e9-6324dd25fec6`, `976deff0-b143-43f3-ae99-eba9493a5b94`, 19.52–19.53) supaya resep _Narfoz_ (`OBT01590`) dapat disimpan; tertagih Rp65.000 | Tinjau: sahkan, ubah, atau nonaktifkan. Itu bukan tarif yang disetujui Billing |
| 5 | **Invoice rawat inap pasien transfer** | Invoice rawat inap pasien K1-D tidak terlihat di bukti uji; penautan invoice IGD → rawat inap tetap ditahan (`IGD-OQ-129`) | Konfirmasi kapan invoice rawat inap lahir sesudah _admit_ transfer, dan jawab `IGD-OQ-129` bila sudah ada sikap |
| 6 | _(tembusan Farmasi)_ **Resep tanpa tarif obat berakhir `500`** | `PrescriptionWorkspaceService.UpsertRegularItemAsync` melempar `InvalidOperationException` yang tidak ditangani (`IGD-FACT-151`) | Pemilik Farmasi: ubah menjadi penolakan `400` berpesan. Untuk demo: pakai obat yang sudah bertarif |
| 7 | **Jalur iuran → membership aktif** | Lihat [`IGD-REQ-005`](2026-10-18-permintaan-pemilik-crm-billing-aktivasi-membership.md) butir 1 (`CRM-DEC-019`, `020`) | Dijawab di berkas itu |

Catatan kinerja (bukan permintaan): aksi yang menerbitkan fakta tagih memakan 6–15 detik di dev karena jalur fakta → jembatan →
invoice berjalan sinkron di dalam permintaan (`IGD-FACT-153`).

---

## 4. Untuk agent Claude di sisi Billing

Berkas ini adalah titik sambung antara blueprint IGD dan blueprint `billing-kasir`. Bila owner Billing meminta agent
mengerjakannya, agent tersebut:

1. Membaca berkas ini, lalu bagian _Penilaian agent pengembang_ pada
   [`testing/2026-10-18-laporan-uji-putaran-k1.md`](../testing/2026-10-18-laporan-uji-putaran-k1.md) dan
   [catatan bukti uji](../evidence/2026-10-18-catatan-bukti-uji-p1-k1-dan-serah-terima.md) bagian 3–5. Bukti mentahnya ada di
   laptop pemilik IGD dan **sudah dinilai**; jangan meminta uji ulang untuk butir di bagian 2.
2. Mencatat keputusan Billing di blueprint **`billing-kasir`** (decision log dan, bila perlu, `approval-requests/`) dengan
   rujukan silang `IGD-REQ-006` dan ID IGD terkait, memakai penomoran Billing sendiri (`BKC-…`).
3. **Tidak** menulis berkas lain di `docs/module-blueprints/igd/**` selain tabel jawaban di bagian 5, dan **tidak** mengubah
   kode IGD maupun alur yang diputuskan `IGD-DEC-242`/`326`.
4. Bila keputusan Billing mengubah kontrak tagihan IGD, menuliskannya sebagai permintaan balik ke pemilik IGD (Rizki), bukan
   langsung mengubah artefak IGD.

---

## 5. Jawaban owner Billing

| Butir | Jawaban | Rujukan di blueprint Billing | Siapa / tanggal |
|---|---|---|---|
| 1 — harga dan kategori jasa kunjungan | | | |
| 2 — porsi penjamin | | | |
| 3 — biaya administrasi IGD | | | |
| 4 — dua tarif obat penguji | | | |
| 5 — invoice rawat inap transfer, `IGD-OQ-129` | | | |
| 6 — `500` resep (Farmasi) | | | |
