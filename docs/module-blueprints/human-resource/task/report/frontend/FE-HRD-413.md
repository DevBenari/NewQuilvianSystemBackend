# Laporan Perubahan Frontend — `FE-HRD-413`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HRD-413` |
| Judul | Form unit: level otomatis dan petunjuk memilih departemen |
| Slice | `S-FIN-M05` amandemen — Menu Organisasi (`PLAN-REPAIR-HRD-008`, `FIX-HRD-008-05`) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/frontend-roadmap.md` bagian 32 |
| Trace | `HRD-DEC-248`, `HRD-DEC-250`; `ISS-HRD-008-02`, `ISS-HRD-008-T2`, `ISS-HRD-008-T4`; skema `PLAN-REPAIR-HRD-008` 3.5 |
| Contract version | `CreateOrganizationUnitRequest` tidak berubah; `LevelNumber` bawaan DTO = 1 (`[Range(1, int.MaxValue)]`) dan ditimpa hasil hitungan server |
| Wewenang UI | Susunan form sesuai skema 3.5 |
| Dependency | `FE-HRD-410` ✅ |
| Klasifikasi | `LIGHT` — 2 berkas konfigurasi/hook |
| Task mode | `FRONTEND` (disetujui pengguna 24 November 2026) |
| Target tulis | `QuilvianSystemFrontendDev/src/**`; laporan ini |
| Model | Claude Opus 5.5 |
| Commit frontend saat dikerjakan | `fe8aebf2f` (branch `HamzahV2`), belum di-commit |
| Commit backend yang dijadikan rujukan | `3ef27c96` (branch `MHamzah`) |
| Tanggal | 2026-11-24 |
| Status | ✅ Selesai — 4/4 AC (AC 2 terbukti dari source server; simpan nyata `NOT RUN`) |

---

## 1. Keadaan yang ditemukan di awal

Form unit mewajibkan "Level Hierarki" (minimal 1), padahal server menghitung level dari unit induk dan mengabaikan isian itu (`OrganizationUnitController.cs:418-440`, `505-516`). Isian relasi berlabel "Parent Unit Organisasi" dan "Departemen Referensi", tanpa petunjuk dari mana departemen berasal.

---

## 2. Proses bisnis dari sisi pengguna

1. Petugas membuka **Struktur Organisasi → + Tambah Unit Organisasi**.
2. Petugas memilih Legal Entity, Fasilitas, dan **Unit Induk**. Di bawah isian tampil "Level unit ditentukan otomatis dari unit induk."
3. Petugas memilih **Departemen Terkait** dari master. Petunjuk di bawah isian: "Pilih dari master Departemen. Departemen baru ditambahkan lewat menu Departemen."
4. Petugas mengisi nama dan tipe, lalu menyimpan. Server menetapkan level, misalnya induk level 2 → unit baru level 3.

Jalur tidak normal: departemen yang dicari belum ada → petugas menambahkannya dulu di menu Departemen. Departemen nonaktif yang dikirim lewat API ditolak server (`BE-HRD-464`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`organization-unit-constants.jsx`, `use-master-data-organization-unit-editor.jsx`, `organization-unit-utils.jsx` (`buildPayload`), `base-form-control.jsx` (render `description`), `OrganizationUnitDtos.cs`, `OrganizationUnitController.cs`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `src/lib/constants/hr/master-data/organization-unit/organization-unit-constants.jsx` | Field `levelNumber` dibuang dari `ORGANIZATION_UNIT_CREATE_FIELDS`; "Unit Induk" + keterangan level otomatis; "Departemen Terkait" + petunjuk master; `helperText` editor memakai istilah "unit induk" |
| `src/lib/hooks/hr/master-data/organization-unit/use-master-data-organization-unit-editor.jsx` | Pengisian otomatis `levelNumber` saat unit induk dipilih dihapus karena field-nya sudah tidak ada; dependency `useCallback` disesuaikan |

### 3.3 Kepatuhan arsitektur frontend

Perubahan hanya pada konfigurasi field dan hook editor domain yang sudah ada. Payload tetap disusun `buildPayload` dari daftar field, jadi `levelNumber` otomatis tidak terkirim.

### 3.4 Gerbang tampilan

| Elemen layar | Base component / sumber gaya | Keputusan | Alasan |
| --- | --- | --- | --- |
| Form unit | `BaseEditorView` / `BaseFormControl` (`description` → `formStyles.hint`) | `REUSE` | — |

`UI GATE: PASS — 1 elemen REUSE, 0 COMPOSE, 0 NEW`

| Layar | Screenshot | Catatan |
| --- | --- | --- |
| `/hr/master-data/organization-unit/create` | `QuilvianSystemFrontendDev/test-with-agy/screenshots/organization-menu-1124/FE-HRD-413-form-unit.png` | Sesuai skema 3.5 |

---

## 4. State yang ditangani di layar

| State | Yang dilihat pengguna |
| --- | --- |
| Memuat | Pilihan relasi memuat (perilaku lama) |
| Kosong | Pilihan departemen hanya "Tidak terkait departemen"; petunjuk menunjukkan tempat menambahkannya |
| Gagal | Galat isian dari server tampil seperti sebelumnya |
| Tanpa hak akses | `AccessDeniedGate` (perilaku lama) |

---

## 5. Endpoint yang dikonsumsi

#### Corporate / Human Resource / Master Data / Organization Unit

| Method | Path | Dipakai untuk | Hak akses |
| --- | --- | --- | --- |
| `POST` | `/v1/corporate/human-resource/master-data/organization-units` | Simpan unit baru (tanpa `levelNumber`) | `OrganizationUnit : Create` |
| `PUT` | `/v1/corporate/human-resource/master-data/organization-units/{id}` | Simpan perubahan unit | `OrganizationUnit : Update` |

---

## 6. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npm run lint:errors` | exit 0 | `PASS` | Keluaran perintah |
| Cek `buildOrganizationUnitPayload` (uji sementara `node --test`, lalu dihapus) | Payload tanpa `levelNumber`, `departmentId` dan `parentOrganizationUnitId` terbawa; masukan `levelNumber: "5"` diabaikan | `PASS` | Keluaran: `{"legalEntityId":…,"departmentId":…,"unitName":"Instalasi Gizi",…}` |
| `npm run build` | Sukses, 927 halaman | `PASS` | Log build |
| Playwright › FE-HRD-413 | Lulus: "Unit Induk" dan petunjuknya, "Departemen Terkait" dan petunjuknya, "Level Hierarki" dan "Parent" 0 kemunculan | `PASS` | 5/5 spec lulus |
| Simpan unit ke backend nyata (AC 2) | Tidak dijalankan | `NOT RUN` | Menulis ke DB dev butuh izin terpisah |

---

## 7. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Form tanpa isian "Level Hierarki" dan payload tanpa `levelNumber` | Terpenuhi | Playwright; cek payload |
| 2. Unit di bawah induk level 2 tersimpan level 3; pindah induk ke level 1 → level 2 | Terpenuhi berdasarkan source server | `calculatedLevel = parentLevel + 1` (create baris 418–440, update 505–516). Simpan nyata `NOT RUN` |
| 3. "Departemen Terkait" memberi petunjuk; pilihan hanya departemen aktif | Terpenuhi | Playwright (petunjuk); pilihan memakai `departments/options` dengan `onlyActive=true` |
| 4. Tidak ada teks "Parent" di form | Terpenuhi | Playwright (0 kemunculan) |
| DoD: laporan dan tanda roadmap | Terpenuhi | Laporan ini; roadmap frontend bagian 32 |

---

## 8. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | 4 warning `react-hooks/set-state-in-effect` lama di hook editor ini (bukan dari task ini) |
| Masalah yang diketahui | — |
| Dependency backend | `BE-HRD-464` menambah penolakan departemen nonaktif di server |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | Lihat `FE-HRD-412` |
| Langkah berikutnya | `FE-HRD-414` |
