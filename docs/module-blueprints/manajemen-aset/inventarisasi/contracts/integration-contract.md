# Inventarisasi Fisik — Kontrak Integrasi

| Field | Value |
|---|---|
| `last_changed_in` | `AST-INV-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

Tidak ada sistem di luar Quilvian dan tidak ada kejadian Akuntansi. Pemindaian QR berjalan di browser
(kamera perangkat) dan hanya mengirim teks nomor inventaris ke backend.

| ID | Arah | Mitra | Service | Saat | Jaminan |
|---|---|---|---|---|---|
| `INT-INV-01` | Memanggil | `register` | `AssetQueryService.GetAssetsForUnit` | Sesi dimulai | Snapshot `Terdaftar` di unit (dan ruangan bila dipilih) |
| `INT-INV-02` | Memanggil | `pergerakan` | `MovementQueryService.HasInFlightForUnit`, daftar pinjaman aktif unit | Sesi dimulai | Baca saja |
| `INT-INV-03` | Memanggil | `register` | `AssetPlacementService` (cek penyerahan berjalan) | Sesi dimulai | Baca saja |
| `INT-INV-04` | Dipanggil | `pergerakan`, `register` | `StockTakeGateQueryService.IsUnitUnderStockTake` | Ajukan mutasi/pinjaman/penyerahan | Baca saja; dipasang task sub-modul ini (`AST-DES-031`) |
| `INT-INV-05` | Memanggil | `pergerakan` | `AssetTransferService.CreateFromStockTake` | Proses `KoreksiLokasi` beda unit | Mutasi `Diajukan` oleh pemroses; persetujuan normal |
| `INT-INV-06` | Memanggil | `register` | `AssetPlacementService.CreateHandover` (`KoreksiOpname`) | Proses `KoreksiLokasi` beda ruangan | Penyerahan `Diserahkan`; konfirmasi normal |
| `INT-INV-07` | Memanggil | `register` | `AssetConditionService.OpenReport` (`TindakLanjutOpname`) | Proses `UsulanHilang`/`PerubahanKondisi` | Laporan `Dilaporkan`; Hilang tetap butuh berita acara |
| `INT-INV-08` | Memanggil | `register` | `AssetRecordService.CreateDraftFromFinding` | Proses `PendaftaranTemuan` | Draft tanpa nomor; foto temuan ikut |
| `INT-INV-09` | Memanggil | `register` | `AssetHistoryService.Append` (`Diopname`) | Sesi diverifikasi | Satu kejadian per aset yang diharapkan |
| `INT-INV-10` | Memanggil | `register` | `AssetAttachmentService` | Foto pindaian/temuan | Konfigurasi fail-closed |

Contoh: sesi `AST-OPN-000007` IGD diverifikasi dengan 2 `TidakDitemukan`, 1 `BedaLokasi` (ditemukan di
ICU), 3 `BedaKondisi`, 1 `TidakTerdaftar`. Lahir 7 usulan. Petugas Aset memproses usulan beda lokasi →
mutasi `AST-MUT-000061` IGD → ICU beralasan `KoreksiOpname`, yang kemudian disetujui seperti mutasi biasa.
