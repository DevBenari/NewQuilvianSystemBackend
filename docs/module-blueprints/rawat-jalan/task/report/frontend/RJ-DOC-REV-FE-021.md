# Laporan Perubahan Frontend — `RJ-DOC-REV-FE-021`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `RJ-DOC-REV-FE-021` |
| Judul | Pencocokan scan kartu di Pembayaran (layar petugas) |
| Roadmap | [doctor-consultation-roadmap.md](../../../roadmap/doctor-consultation-roadmap.md) bagian `19` |
| Trace | `RJ-DOC-DEC-068`..`070`; `03` *PM-FE.2* |
| Contract version | `RJ-DOC-REFERRAL-001@1.0.0` (**approved**) — `cardScan` pada create penjamin asuransi |
| Dependency | `RJ-DOC-REV-BE-020` ✅ |
| Task mode | `CROSS-REPO MODE` — frontend (`RJ-DOC-DEC-083`) |
| Commit frontend | `de323430` (`sukmagpV2`), belum di-commit |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Tanggal | 8 Oktober 2026 |
| Status | ✅ `COMPLETE` |

## 1. Proses dari sisi pengguna

Di langkah Pembayaran, petugas menekan **Daftarkan Penjamin Baru**, lalu memilih asuransi, mengisi No. polis dan masa aktif, kemudian menekan **Scan Kartu**.

| Hasil baca kartu oleh agent | Yang tampil | Simpan |
| --- | --- | --- |
| Agent tidak membaca nama/No. polis (keadaan sekarang) | Hanya gambar kartu, seperti `RJ-DOC-REV-FE-014` | Aktif |
| Nama atau No. polis berbeda | Alert merah **"Data tidak match"** beserta isi kartu yang terbaca | Nonaktif |
| Hanya salah satu terbaca | Alert **"Kartu tidak terbaca lengkap"** | Nonaktif |
| Keduanya cocok (contoh: `PT PRUDENTIAL INDONESIA LIFE` / `pmt 660302 01` untuk Prudential Indonesia `PMT-660302-01`) | Alert hijau **"Kartu cocok"** | Aktif; `cardScan` ikut terkirim dan diperiksa ulang backend |

Ganti jenis atau penyedia penjamin, batalkan pilihan, atau hapus gambar akan mengosongkan hasil baca. Penjamin perusahaan tidak dicocokkan, sesuai `RJ-DOC-DEC-069`.

## 2. Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/utils/shared/insurance-card-scan-utils.jsx` | Baru — `matchInsuranceCardScan`, `readInsuranceCardScanFields`, normalisasi; aturan sama dengan `InsuranceCardScanMatcher` backend. Dipakai ulang `FE-KSK-016` |
| `src/lib/hooks/…/emergency-registration/use-plustek-payer-card-scanner.js` | `scanCard()` mengembalikan `{ imageDataUrl, cardScan }`; `scanCardImage()` lama tetap ada |
| `src/components/view/…/emergency-registration/emergency-patient-payer-modal.jsx` | Status pencocokan, alert, kunci tombol Simpan, `cardScan` pada submit |
| `src/utils/…/emergency-registration.utils.js` | `buildPatientInsurancePayload` mengirim `cardScan` |
| `src/lib/hooks/…/outpatient-registration/use-outpatient-registration.js` | `payerCard.scanCard` diteruskan ke modal |

Pendaftaran IGD tidak berubah: modal hanya mencocokkan bila `cardScanner` dikirim, dan IGD tidak mengirimnya (`RJ-DOC-REV-FE-014`).

**UI GATE: 2 elemen — REUSE 2, NEW 0** (`EmergencyInlineAlert` tone `error`/`success`, tombol yang sudah ada).

## 3. Kontrak OCR yang diusulkan (`RJ-DOC-OQ-PM-01`)

Agent belum membaca kartu asuransi. Layar membaca `ocr.fields` pada akar atau halaman pertama respons `POST /scanner/scan` dengan kunci berikut (tidak peka huruf besar-kecil). Daftar ini adalah **usulan** yang perlu disepakati dengan tim agent:

| Nilai | Kunci yang diterima |
| --- | --- |
| Nama asuransi | `InsuranceName`, `InsuranceProviderName`, `ProviderName`, `NamaAsuransi` |
| No. polis | `PolicyNumber`, `NoPolis`, `NomorPolis`, `CardNumber` |

## 4. Verifikasi

| Skenario / perintah | Hasil | Klasifikasi |
| --- | --- | --- |
| `npx eslint` 5 berkas | 0 error; warning sama dengan `HEAD` (modal 2 warning lama) | `PASS` |
| `npm run build` | exit 0 | `PASS` |
| Uji layar Playwright (build 3100 → backend uji 7185, agent Plustek **tiruan**) | **6/6 PASS** | `PASS` |

| ID | Skenario | Hasil |
| --- | --- | --- |
| F1 | Agent tanpa field → tanpa alert, Simpan aktif | PASS |
| F2 | No. polis beda → "Data tidak match", Simpan nonaktif | PASS |
| F3 | Nama asuransi beda (Allianz) → "Data tidak match" | PASS |
| F4 | Hanya nama terbaca → "Kartu tidak terbaca lengkap", Simpan nonaktif | PASS |
| F5 | Cocok (polis beda format) → "Kartu cocok" | PASS |
| F6 | Simpan mengirim `cardScan`; backend `200` | PASS |

Uji pertama gagal 5/6 karena `payerCard` belum meneruskan `scanCard`. Itu diperbaiki, lalu diulang 6/6. Dua penjamin uji dihapus (`DELETE …/admin/{id}`, `200`).

`AUTOMATED TEST: SKIPPED (opsional) — tidak ada test baru.`

## 5. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. `RJ-AC-PM-01`/`02` dengan agent tiruan | Terpenuhi | F2–F6 |
| 2. `RJ-AC-PM-03` penjamin tersimpan tidak diminta scan | Terpenuhi — pencocokan hanya ada di modal penjamin baru; scan kartu penjamin tersimpan tetap memakai `scanCardImage` tanpa pencocokan | Source |
| 3. Agent tanpa nilai → perilaku `FE-014` identik | Terpenuhi | F1 |
| 4. Lint, build | Terpenuhi | Bagian 4 |

## 6. Catatan penutup

| Hal | Isi |
| --- | --- |
| Risiko | Pencocokan sungguhan menunggu kontrak OCR agent (`RJ-DOC-OQ-PM-01`) |
| Perubahan sampingan | `NONE` |
